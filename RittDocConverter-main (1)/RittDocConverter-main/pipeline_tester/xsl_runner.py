"""
XSL Transformation Runner

Provides a wrapper around lxml's XSLT processor with support for:
- Capturing xsl:message outputs
- Error handling and reporting
- Optional Saxon fallback for advanced XSLT features
"""

import os
import sys
import subprocess
import tempfile
from pathlib import Path
from typing import Optional, Tuple, List, Dict, Any
from dataclasses import dataclass, field
from lxml import etree
import logging

logger = logging.getLogger(__name__)


@dataclass
class TransformResult:
    """Result of an XSL transformation"""
    success: bool
    output_xml: Optional[str] = None
    output_tree: Optional[etree._Element] = None
    messages: List[str] = field(default_factory=list)
    warnings: List[str] = field(default_factory=list)
    errors: List[str] = field(default_factory=list)
    duration_ms: float = 0.0


class MessageCapture:
    """Captures xsl:message outputs during transformation"""

    def __init__(self):
        self.messages: List[str] = []
        self.errors: List[str] = []

    def __call__(self, message):
        """Called by lxml for each xsl:message"""
        msg_text = str(message)
        if 'error' in msg_text.lower() or 'fatal' in msg_text.lower():
            self.errors.append(msg_text)
        else:
            self.messages.append(msg_text)


class XSLRunner:
    """Runs XSL transformations with error capture and reporting"""

    def __init__(self, xsl_dir: Path):
        self.xsl_dir = Path(xsl_dir)
        self.saxon_path = self._find_saxon()

    def _find_saxon(self) -> Optional[Path]:
        """Try to find Saxon-HE for advanced XSLT features"""
        # Check common locations
        possible_paths = [
            Path("/usr/share/java/saxon-he.jar"),
            Path("/usr/share/java/Saxon-HE.jar"),
            Path.home() / ".m2/repository/net/sf/saxon/Saxon-HE",
            Path("/opt/saxon/saxon-he.jar"),
        ]

        for path in possible_paths:
            if path.exists():
                if path.is_dir():
                    # Find latest version
                    jars = list(path.glob("**/Saxon-HE*.jar"))
                    if jars:
                        return jars[0]
                else:
                    return path

        # Try to find via which
        try:
            result = subprocess.run(
                ["which", "saxon"],
                capture_output=True,
                text=True
            )
            if result.returncode == 0:
                return Path(result.stdout.strip())
        except Exception:
            pass

        return None

    def transform_lxml(
        self,
        xml_input: Path,
        xsl_file: str,
        params: Optional[Dict[str, str]] = None
    ) -> TransformResult:
        """
        Transform XML using lxml's XSLT processor.

        Args:
            xml_input: Path to input XML file
            xsl_file: Name of XSL file (relative to xsl_dir)
            params: Optional parameters to pass to the stylesheet

        Returns:
            TransformResult with success status, output, and messages
        """
        import time
        start_time = time.time()

        result = TransformResult(success=False)
        xsl_path = self.xsl_dir / xsl_file

        if not xsl_path.exists():
            result.errors.append(f"XSL file not found: {xsl_path}")
            return result

        try:
            # Parse input XML
            parser = etree.XMLParser(
                remove_blank_text=False,
                resolve_entities=False,
                load_dtd=False,
                no_network=True
            )

            try:
                xml_doc = etree.parse(str(xml_input), parser)
            except etree.XMLSyntaxError as e:
                result.errors.append(f"XML parse error in input: {e}")
                return result

            # Parse XSL
            try:
                xsl_doc = etree.parse(str(xsl_path), parser)
            except etree.XMLSyntaxError as e:
                result.errors.append(f"XSL parse error in {xsl_file}: {e}")
                return result

            # Create transformer with message capture
            msg_capture = MessageCapture()

            try:
                transform = etree.XSLT(xsl_doc)
            except etree.XSLTParseError as e:
                result.errors.append(f"XSL compile error: {e}")
                for error in e.error_log:
                    result.errors.append(f"  Line {error.line}: {error.message}")
                return result

            # Prepare parameters
            xslt_params = {}
            if params:
                for key, value in params.items():
                    # XSLT params need to be quoted strings
                    xslt_params[key] = etree.XSLT.strparam(value)

            # Run transformation
            try:
                output = transform(xml_doc, **xslt_params)

                # Capture messages from error log
                for entry in transform.error_log:
                    msg = str(entry.message)
                    if entry.level_name == 'ERROR':
                        result.errors.append(msg)
                    elif entry.level_name == 'WARNING':
                        result.warnings.append(msg)
                    else:
                        result.messages.append(msg)

                result.output_tree = output.getroot() if output.getroot() is not None else None
                result.output_xml = etree.tostring(
                    output,
                    encoding='unicode',
                    pretty_print=True
                ) if output.getroot() is not None else str(output)
                result.success = True

            except etree.XSLTApplyError as e:
                result.errors.append(f"XSL apply error: {e}")
                for error in transform.error_log:
                    if error.level_name == 'ERROR':
                        result.errors.append(f"  {error.message}")

        except Exception as e:
            result.errors.append(f"Unexpected error: {type(e).__name__}: {e}")

        result.duration_ms = (time.time() - start_time) * 1000
        return result

    def transform_saxon(
        self,
        xml_input: Path,
        xsl_file: str,
        output_file: Optional[Path] = None,
        params: Optional[Dict[str, str]] = None
    ) -> TransformResult:
        """
        Transform XML using Saxon (required for xsl:document, saxon:evaluate, etc.)

        Args:
            xml_input: Path to input XML file
            xsl_file: Name of XSL file (relative to xsl_dir)
            output_file: Optional path to write output
            params: Optional parameters to pass to the stylesheet

        Returns:
            TransformResult with success status, output, and messages
        """
        import time
        start_time = time.time()

        result = TransformResult(success=False)
        xsl_path = self.xsl_dir / xsl_file

        if not self.saxon_path:
            result.errors.append("Saxon not found - required for this transformation")
            return result

        if not xsl_path.exists():
            result.errors.append(f"XSL file not found: {xsl_path}")
            return result

        # Build Saxon command
        with tempfile.NamedTemporaryFile(mode='w', suffix='.xml', delete=False) as tmp:
            tmp_output = tmp.name

        try:
            cmd = [
                "java", "-jar", str(self.saxon_path),
                f"-s:{xml_input}",
                f"-xsl:{xsl_path}",
                f"-o:{output_file or tmp_output}"
            ]

            # Add parameters
            if params:
                for key, value in params.items():
                    cmd.append(f"{key}={value}")

            # Run Saxon
            proc = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=300  # 5 minute timeout
            )

            # Capture stderr as messages/errors
            if proc.stderr:
                for line in proc.stderr.strip().split('\n'):
                    if line:
                        if 'error' in line.lower():
                            result.errors.append(line)
                        elif 'warning' in line.lower():
                            result.warnings.append(line)
                        else:
                            result.messages.append(line)

            if proc.returncode == 0:
                # Read output
                output_path = output_file or Path(tmp_output)
                if output_path.exists():
                    result.output_xml = output_path.read_text(encoding='utf-8')
                    try:
                        result.output_tree = etree.fromstring(
                            result.output_xml.encode('utf-8')
                        )
                    except Exception:
                        pass  # Output may not be well-formed XML
                result.success = True
            else:
                result.errors.append(f"Saxon exited with code {proc.returncode}")
                if proc.stdout:
                    result.errors.append(proc.stdout)

        except subprocess.TimeoutExpired:
            result.errors.append("Saxon transformation timed out after 5 minutes")
        except Exception as e:
            result.errors.append(f"Saxon execution error: {e}")
        finally:
            # Cleanup temp file
            if not output_file and os.path.exists(tmp_output):
                os.unlink(tmp_output)

        result.duration_ms = (time.time() - start_time) * 1000
        return result

    def transform(
        self,
        xml_input: Path,
        xsl_file: str,
        output_file: Optional[Path] = None,
        params: Optional[Dict[str, str]] = None,
        require_saxon: bool = False
    ) -> TransformResult:
        """
        Transform XML using the best available processor.

        Will use lxml by default, falling back to Saxon if needed or requested.

        Args:
            xml_input: Path to input XML file
            xsl_file: Name of XSL file (relative to xsl_dir)
            output_file: Optional path to write output
            params: Optional parameters to pass to the stylesheet
            require_saxon: Force use of Saxon processor

        Returns:
            TransformResult with success status, output, and messages
        """
        if require_saxon:
            return self.transform_saxon(xml_input, xsl_file, output_file, params)

        # Try lxml first
        result = self.transform_lxml(xml_input, xsl_file, params)

        # If failed due to unsupported features, try Saxon
        if not result.success:
            saxon_features = ['xsl:document', 'saxon:', 'exslt:']
            xsl_content = (self.xsl_dir / xsl_file).read_text()

            if any(feature in xsl_content for feature in saxon_features):
                logger.info(f"Retrying {xsl_file} with Saxon due to advanced features")
                result = self.transform_saxon(xml_input, xsl_file, output_file, params)

        # Write output if requested
        if result.success and output_file and result.output_xml:
            output_file.parent.mkdir(parents=True, exist_ok=True)
            output_file.write_text(result.output_xml, encoding='utf-8')

        return result


def test_xsl_runner():
    """Quick test of the XSL runner"""
    runner = XSLRunner(Path(__file__).parent / "xsl")

    print(f"XSL Directory: {runner.xsl_dir}")
    print(f"Saxon available: {runner.saxon_path}")

    # List available XSL files
    print("\nAvailable XSL files:")
    for xsl in runner.xsl_dir.glob("*.xsl"):
        print(f"  - {xsl.name}")


if __name__ == "__main__":
    test_xsl_runner()
