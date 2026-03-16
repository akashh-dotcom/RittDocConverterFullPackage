<?xml version='1.0'?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version='1.0'>

<xsl:template name="admon.graphic.width">
  <xsl:param name="node" select="."/>
  <xsl:text>25</xsl:text>
</xsl:template>
<xsl:template match="note|sidebar|important|warning|caution|tip" mode="anticipated_posfilter"	>
<xsl:param name="posfilter" select="0"	/>	
<xsl:choose>
	<xsl:when test="position() = 1">
		<xsl:if test="$posfilter = 1"><xsl:apply-templates select="." mode="anticipated"	/></xsl:if>	
	</xsl:when>
	<xsl:otherwise><xsl:if test="$posfilter = 2">
		<xsl:choose>
			<xsl:when test="count(../note|sidebar|important|warning|caution|tip) = 2">
				<xsl:apply-templates select="." mode="anticipated"	/>	
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="." mode="anticipated">
					<xsl:with-param  name="delayed" select="2" />		
				</xsl:apply-templates>
			</xsl:otherwise>	
			</xsl:choose>
			</xsl:if>
			</xsl:otherwise>			
</xsl:choose>	
</xsl:template>	

<xsl:template match="note|sidebar|important|warning|caution|tip" mode="anticipated">
	<xsl:param name="delayed" ></xsl:param>		
	<xsl:choose>	  
    <xsl:when test="$admon.graphics != 0">
      <xsl:call-template name="graphical.admonition"/>
    </xsl:when>
    <xsl:otherwise>
		  <xsl:call-template name="nongraphical.admonition" >
			<xsl:with-param name="delayed" select="$delayed" />		
		</xsl:call-template>	  
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template name="admon.graphic">
  <xsl:param name="node" select="."/>
  <xsl:value-of select="$admon.graphics.path"/>
  <xsl:choose>
    <xsl:when test="local-name($node)='note'">note</xsl:when>
    <xsl:when test="local-name($node)='warning'">warning</xsl:when>
    <xsl:when test="local-name($node)='caution'">caution</xsl:when>
    <xsl:when test="local-name($node)='tip'">tip</xsl:when>
    <xsl:when test="local-name($node)='important'">important</xsl:when>
    <xsl:when test="local-name($node)='sidebar'">sidebar</xsl:when>
    <xsl:otherwise>note</xsl:otherwise>
  </xsl:choose>
  <xsl:value-of select="$admon.graphics.extension"/>
</xsl:template>

<xsl:template name="graphical.admonition">
  <xsl:variable name="admon.type">
    <xsl:choose>
      <xsl:when test="local-name(.)='note'">Note</xsl:when>
      <xsl:when test="local-name(.)='warning'">Warning</xsl:when>
      <xsl:when test="local-name(.)='caution'">Caution</xsl:when>
      <xsl:when test="local-name(.)='tip'">Tip</xsl:when>
      <xsl:when test="local-name(.)='important'">Important</xsl:when>
      <xsl:when test="local-name(.)='sidebar'">Sidebar</xsl:when>
      <xsl:otherwise>Note</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

    <table border="0">
      <xsl:attribute name="summary">
        <xsl:value-of select="$admon.type"/>
        <xsl:if test="title">
          <xsl:text>: </xsl:text>
          <xsl:value-of select="title"/>
        </xsl:if>
      </xsl:attribute>
      <tr>
        <td rowspan="2" align="center" valign="top">
          <xsl:attribute name="width">
            <xsl:call-template name="admon.graphic.width"/>
          </xsl:attribute>
          <!--<img alt="[{$admon.type}]">-->
          <xsl:element name="img">
            <xsl:attribute name="src">
              <xsl:call-template name="admon.graphic"/>
            </xsl:attribute>
            </xsl:element>
        </td>
        <th align="left">
          <xsl:call-template name="anchor"/>
          <xsl:if test="$admon.textlabel != 0 or title">
            <xsl:apply-templates select="." mode="object.title.markup"/>
          </xsl:if>
        </th>
      </tr>
      <tr>
        <td colspan="2" align="left" valign="top">
          <xsl:apply-templates/>
        </td>
      </tr>
    </table>
</xsl:template>

<xsl:template name="nongraphical.admonition">
  <div class="featurebox">
    <xsl:attribute name="id"><xsl:value-of select="./@id" /></xsl:attribute>
    <div class="featureboxinner">
      <xsl:if test="$admon.textlabel != 0 or title">
        <div class="featureboxheader"><xsl:call-template name="sect2.titlepage" /></div>
      </xsl:if>
      <xsl:apply-templates/>
    </div>
  </div>
</xsl:template>


<!-- ## 12/11/05 Match to show the boxes at the original position ## -->
<!-- ## 15/11/05 Match to remove boxes if parent is para which handled in Block xsl para match ## -->
<xsl:template match="note|sidebar|important|warning|caution|tip">
	<xsl:choose>
		<xsl:when test="parent::para"></xsl:when>
		<xsl:otherwise>
			<xsl:apply-templates select="."	mode="anticipated"	/>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>	


<xsl:template match="sect2">
	<!-- |important|warning|caution|tip -->
 <!--<div class="{name(.)}">
  <xsl:call-template name="language.attribute" />-->

	<xsl:variable name="isVideoSection">
		<xsl:call-template name="is.video.section" />
	</xsl:variable>

	<xsl:call-template name="begin.section">
		<xsl:with-param name="isVideoSection" select="$isVideoSection" />
	</xsl:call-template>

  <xsl:call-template name="sect2.titlepage" /> 
 <xsl:variable name="toc.params">
 <xsl:call-template name="find.path.params">
  <xsl:with-param name="table" select="normalize-space($generate.toc)" /> 
  </xsl:call-template>
  </xsl:variable>
 <xsl:if test="contains($toc.params, 'toc') and $generate.section.toc.level >= 2">
 <xsl:call-template name="section.toc">
  <xsl:with-param name="toc.title.p" select="contains($toc.params, 'title')" /> 
  </xsl:call-template>
  <xsl:call-template name="section.toc.separator" /> 
  </xsl:if>
  <!--</div>-->
  <xsl:apply-templates /> 
  <xsl:call-template name="process.chunk.footnotes" />

	<xsl:call-template name="end.section">
		<xsl:with-param name="isVideoSection" select="$isVideoSection" />
	</xsl:call-template>
</xsl:template>
  
 <!-- some other suppressions -->
<xsl:template match="note/title"></xsl:template>
<xsl:template match="sidebar/title"></xsl:template>
<xsl:template match="important/title"></xsl:template>
<xsl:template match="warning/title"></xsl:template>
<xsl:template match="caution/title"></xsl:template>
<xsl:template match="tip/title"></xsl:template>
<xsl:template match="*" mode="anticipated"></xsl:template>	
</xsl:stylesheet>