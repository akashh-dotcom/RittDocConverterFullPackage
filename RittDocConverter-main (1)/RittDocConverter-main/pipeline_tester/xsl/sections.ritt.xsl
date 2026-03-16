<?xml version='1.0'?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version='1.0'>
<!-- titlepage_ritt.xsl -->
<!-- extentions for section title pages -->
<!-- sect6 info area -->
<xsl:template match="sect6|sect7|sect8|sect9|sect10">
  <!--<div class="{name(.)}">
    <xsl:call-template name="language.attribute"/>-->
    <xsl:variable name="sectlevel">
		<xsl:choose>
			<xsl:when test="name(.)='sect6'">6</xsl:when>
			<xsl:when test="name(.)='sect7'">7</xsl:when>
			<xsl:when test="name(.)='sect8'">8</xsl:when>
			<xsl:when test="name(.)='sect9'">9</xsl:when>
			<xsl:when test="name(.)='sect10'">10</xsl:when>
			<xsl:otherwise>11</xsl:otherwise>	
		</xsl:choose>	
    </xsl:variable>	
		<xsl:choose>
			<xsl:when test="name(.)='sect6'"><xsl:call-template name="sect6.titlepage"/></xsl:when>
			<xsl:when test="name(.)='sect7'"><xsl:call-template name="sect7.titlepage"/></xsl:when>
			<xsl:when test="name(.)='sect8'"><xsl:call-template name="sect8.titlepage"/></xsl:when>
			<xsl:when test="name(.)='sect9'"><xsl:call-template name="sect9.titlepage"/></xsl:when>
			<xsl:when test="name(.)='sect10'"><xsl:call-template name="sect10.titlepage"/></xsl:when>
		</xsl:choose>	
    

    <xsl:variable name="toc.params">
      <xsl:call-template name="find.path.params">
        <xsl:with-param name="table" select="normalize-space($generate.toc)"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:if test="contains($toc.params, 'toc')
                  and $generate.section.toc.level &gt;= $sectlevel">
      <xsl:call-template name="section.toc">
        <xsl:with-param name="toc.title.p" select="contains($toc.params, 'title')"/>
      </xsl:call-template>
      <xsl:call-template name="section.toc.separator"/>
    </xsl:if>
    
    <xsl:apply-templates/>
    <xsl:call-template name="process.chunk.footnotes"/>
  <!--</div>-->
</xsl:template>

<xsl:template match="sect6/title|sect7/title|sect8/title|sect9/title|sect10/title"></xsl:template>

<xsl:template match="sect6/subtitle|
				sect7/subtitle|
				sect8/subtitle|
				sect9/subtitle|
				sect10/subtitle"></xsl:template>
<xsl:template match="sect6/titleabbrev|
				sect7/titleabbrev|
				sect8/titleabbrev|
				sect9/titleabbrev|
				sect10/titleabbrev"></xsl:template>
<xsl:template match="sect6info|sect7info|sect8info|sect9info|sect10info"></xsl:template>


<xsl:template match="sect6|sect7|sect8|sect9|sect10"
              mode="object.title.template">
  <xsl:choose>
    <xsl:when test="$section.autolabel != 0">
      <xsl:call-template name="gentext.template">
        <xsl:with-param name="context" select="'title-numbered'"/>
        <xsl:with-param name="name">
          <xsl:call-template name="xpath.location"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:when>
    <xsl:otherwise>
      <xsl:call-template name="gentext.template">
        <xsl:with-param name="context" select="'title-unnumbered'"/>
        <xsl:with-param name="name">
          <xsl:call-template name="xpath.location"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:otherwise>
  </xsl:choose>  
</xsl:template>

<xsl:template name="begin.section">
	<xsl:param name="isVideoSection" select="0"/>

	<xsl:if test="$isVideoSection = 1">
		<xsl:comment> field: r2VideoSection</xsl:comment>
		<xsl:message>{ "messageType": "video", "isbn": "<xsl:value-of select="$isbndir" />", "section": "<xsl:value-of select="/*/@id" />", "mediaUrl": "<xsl:value-of select="//videodata/@fileref" />"}</xsl:message>
	</xsl:if>
</xsl:template>

<xsl:template name="end.section">
	<xsl:param name="isVideoSection" select="0"/>

	<xsl:if test="$isVideoSection = 1">
		<xsl:comment> field: </xsl:comment>
	</xsl:if>
</xsl:template>

<xsl:template name="is.leaf.section">
	<xsl:param name="node" select="."/>
	<xsl:choose>
		<xsl:when test="count($node[descendant::section|sect1|sect2|sect3|sect4|sect5|refsect1|refsect2|refsect3|simplesect]) = 0">1</xsl:when>
		<xsl:otherwise>0</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="is.video.section">
	<xsl:param name="node" select="."/>

	<xsl:variable name="isLeafSection">
		<xsl:call-template name="is.leaf.section">
			<xsl:with-param name="node" select="$node" />
		</xsl:call-template>
  </xsl:variable>

	<xsl:choose>
		<xsl:when test="$isLeafSection = 1 and count($node[descendant::videoobject]) > 0">1</xsl:when>
		<xsl:otherwise>0</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="in.video.section">
	<xsl:param name="node"
		select="(ancestor::section
						|ancestor::sect1
						|ancestor::sect2
						|ancestor::sect3
						|ancestor::sect4
						|ancestor::sect5
						|ancestor::refsect1
						|ancestor::refsect2
						|ancestor::refsect3
						|ancestor::simplesect)[last()]"/>

	<xsl:call-template name="is.video.section">
		<xsl:with-param name="node" select="$node" />
	</xsl:call-template>
</xsl:template>

</xsl:stylesheet>
