<?xml version='1.0'?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version='1.0'
                xmlns:str="http://xsltsl.org/string" 
                exclude-result-prefixes="str xsl"	
                >
<!-- link.ritt.xsl -->
<!-- extensions for link -->
  
<xsl:template match="link" name="link">
  <xsl:param name="a.target"/>
  <xsl:variable name="targets" select="key('id',@linkend)"/>
  <xsl:variable name="target" select="$targets[1]"/>
  <xsl:variable name="hreflink" ><xsl:call-template name="href.target"><xsl:with-param name="object" select="$target"/></xsl:call-template></xsl:variable>	
  <xsl:variable name="popupLink">
    <xsl:if test="@linkend != '' and string-length(@linkend) &gt; 5">
      <xsl:choose>
	      <xsl:when test="substring-after(@linkend , 'fg' ) != '' " >fg<xsl:value-of select="substring-after(@linkend , 'fg' )"	/></xsl:when>
	      <xsl:when test="substring-after(@linkend , 'eq' ) != '' " >eq<xsl:value-of select="substring-after(@linkend , 'eq' )"	/></xsl:when>
	      <xsl:when test="substring-after(@linkend , 'ta' ) != '' " >ta<xsl:value-of select="substring-after(@linkend , 'ta' )"	/></xsl:when>
	      <xsl:when test="substring-after(@linkend , 'gl' ) != '' " >gl<xsl:value-of select="substring-after(@linkend , 'gl' )"	/></xsl:when>
	      <xsl:when test="substring-after(@linkend , 'bib' ) != '' " >bib<xsl:value-of select="substring-after(@linkend , 'bib' )" /></xsl:when>
	      <xsl:when test="substring-after(@linkend , 'qa' ) != '' " >qa<xsl:value-of select="substring-after(@linkend , 'qa' )"	/></xsl:when>
	      <xsl:when test="substring-after(@linkend , 'pr' ) != '' " >pr<xsl:value-of select="substring-after(@linkend , 'pr' )"	/></xsl:when>
        <xsl:when test="substring-after(@linkend , 'vd' ) != '' " >vd<xsl:value-of select="substring-after(@linkend , 'vd' )"	/></xsl:when>
        <xsl:when test="substring-after(@linkend , 'ad' ) != '' " >ad<xsl:value-of select="substring-after(@linkend , 'ad' )"	/></xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>	
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="sectionlink">
    <xsl:choose>
	    <xsl:when test="@linkend != '' and substring-after(@linkend , 'ch' ) != ''  and substring-after(@linkend , 's' ) != '' and $popupLink != '' and (string-length(substring-after(@linkend , 's' )) &gt; string-length($popupLink ) )">s<xsl:value-of select="translate(substring-before(substring-after(@linkend , 's' ), $popupLink), '_', '' )"	/></xsl:when>	
	    <xsl:when test="@linkend != '' and substring-after(@linkend , 'ch' ) != ''  and substring-after(@linkend , 's' ) != '' and $popupLink = '' ">s<xsl:value-of select="substring-after(@linkend , 's' )"	/></xsl:when>
      <!--** 11-01-2008 updated for appendix linking with in section -->
		  <xsl:when test="@linkend != '' and substring-after(@linkend , 'ap' ) != ''  and substring-after(@linkend , 's' ) != '' and $popupLink = '' ">s<xsl:value-of select="substring-after(@linkend , 's' )"	/></xsl:when>
    </xsl:choose>
  </xsl:variable>	
  <xsl:variable name="chapterlink">
	  <xsl:choose>
		  <xsl:when test="$sectionlink != ''"><xsl:value-of select="substring-before(@linkend , $sectionlink)" /></xsl:when>
		  <xsl:when test="$popupLink != ''"><xsl:value-of select="substring-before(@linkend , $popupLink)" /></xsl:when>
		  <xsl:otherwise><xsl:value-of select="@linkend"	/></xsl:otherwise>
	  </xsl:choose>
  </xsl:variable>	
  <xsl:variable name="chapterlen">
    <xsl:choose>
	    <xsl:when test="$chapterlink != '' "><xsl:value-of select="string-length ($chapterlink) "/>	</xsl:when>
	    <xsl:otherwise >6</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>	
  <xsl:variable name="chaptsectlen">
    <xsl:choose>
	    <xsl:when test="$chapterlink != '' and $sectionlink != '' "><xsl:value-of select="$chapterlen + string-length ($sectionlink)"/></xsl:when>
	    <xsl:otherwise>12</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>	
  
  <xsl:choose>
		<xsl:when test="$hreflink ='#' and $sectionlink != '' and (local-name($target) =  'note' or local-name($target) =  'important' or local-name($target) =  'sidebar' or local-name($target) =  'warning' or local-name($target) =  'caution' or local-name($target) =  'tip')"><xsl:apply-templates /></xsl:when>
	  <xsl:otherwise>
    <a><xsl:attribute name="href">
		  <xsl:choose>
	          <xsl:when test="substring($hreflink,1,1) ='#' and @linkend != ''">
	          	<xsl:choose>
					<xsl:when test="@linkend != '' and (substring-after(@linkend , 'ap' ) != '' or substring-after(@linkend , 'pr' ) != '' or substring-after(@linkend , 'pt' ) != '')  and substring-after(@linkend , 's' ) != ''"><xsl:value-of select="substring-before(@linkend , 's' )" />#goto=<xsl:value-of select="@linkend"/></xsl:when>
					<xsl:when test="@linkend != '' and substring-after(@linkend , 'ap' ) != ''"><xsl:value-of select="substring(@linkend,1,$chapterlen)"/>#goto=<xsl:value-of select="@linkend"/></xsl:when>
					<xsl:when test="(string-length(@linkend) = $chapterlen or string-length(@linkend) = $chaptsectlen) and not(contains(@linkend,'note'))"><xsl:value-of select="@linkend"/></xsl:when>
					<xsl:otherwise><xsl:value-of select="substring(@linkend,1,11)"/>#goto=<xsl:value-of select="@linkend" /></xsl:otherwise>
	          	</xsl:choose>
	          </xsl:when>
			  <xsl:otherwise><xsl:value-of select="$hreflink" /></xsl:otherwise>
		  </xsl:choose>
	    </xsl:attribute>
			
		  <!-- FIXME: is there a better way to tell what elements have a title? -->
		  <xsl:if test="local-name($target) = 'book'
							    or local-name($target) = 'set'
							    or local-name($target) = 'chapter'
							    or local-name($target) = 'preface'
							    or local-name($target) = 'appendix'
							    or local-name($target) = 'bibliography'
							    or local-name($target) = 'glossary'
							    or local-name($target) = 'index'
							    or local-name($target) = 'part'
							    or local-name($target) = 'refentry'
							    or local-name($target) = 'reference'
							    or local-name($target) = 'example'
							    or local-name($target) = 'equation'
							    or local-name($target) = 'table'
							    or local-name($target) = 'figure'
							    or local-name($target) = 'qandaset'
							    or local-name($target) = 'simplesect'
							    or starts-with(local-name($target),'sect')
							    or starts-with(local-name($target),'refsect')">
	      <xsl:attribute name="title"><xsl:value-of select="$target/title/node()"	/></xsl:attribute>
		  </xsl:if>
			
		  <xsl:choose>
			  <xsl:when test="count(child::node()) &gt; 0">
				  <!-- If it has content, use it -->
				  <xsl:apply-templates/>
			  </xsl:when>
			  <xsl:otherwise>
				  <!-- else look for an endterm -->
				  <xsl:choose>
					  <xsl:when test="@endterm">
					    <xsl:variable name="etargets" select="key('id',@endterm)"/>
						  <xsl:variable name="etarget" select="$etargets[1]"/>
						  <xsl:choose>
						    <xsl:when test="count($etarget) = 0">
							    <xsl:message>
							      <xsl:value-of select="count($etargets)"/>
							      <xsl:text>Endterm points to nonexistent ID: </xsl:text>
							      <xsl:value-of select="@endterm"/>
							    </xsl:message>
							    <xsl:text>???</xsl:text>
						    </xsl:when>
						    <xsl:otherwise>
							    <xsl:apply-templates select="$etarget" mode="endterm"/>
						    </xsl:otherwise>
						  </xsl:choose>
					  </xsl:when>
					  <xsl:otherwise>
						  <xsl:message>
						    <xsl:text>Link element has no content and no Endterm. </xsl:text>
						    <xsl:text>Nothing to show in the link to </xsl:text>
						    <xsl:value-of select="$target"/>
						  </xsl:message>
						  <xsl:text>???</xsl:text>
					  </xsl:otherwise>
				  </xsl:choose>
		    </xsl:otherwise>
		  </xsl:choose>
	  </a>
	  </xsl:otherwise>
  </xsl:choose>	
</xsl:template>
</xsl:stylesheet>