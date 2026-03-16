<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:doc="http://nwalsh.com/xsl/documentation/1.0"
                exclude-result-prefixes="doc"
                version='1.0'>

<xsl:template match="subpart" mode="is.autonumber">
  <xsl:value-of select="$part.autolabel"/>
</xsl:template>

<xsl:template match="subpart" mode="label.markup">
  <xsl:choose>
    <xsl:when test="@label">
      <xsl:value-of select="@label"/>
    </xsl:when>
    <xsl:when test="$part.autolabel != 0">
      <xsl:number from="book" count="part" format="I"/>
    </xsl:when>
  </xsl:choose>
</xsl:template>

<xsl:template match="subpart" mode="subtitle.markup">
  <xsl:param name="allow-anchors" select="'0'"/>
  <xsl:apply-templates select="(partinfo/subtitle
                                |docinfo/subtitle
                                |info/subtitle
                                |subtitle)[1]"
                       mode="subtitle.markup">
    <xsl:with-param name="allow-anchors" select="$allow-anchors"/>
  </xsl:apply-templates>
</xsl:template>

<xsl:template match="subpart" mode="title.markup">
  <xsl:param name="allow-anchors" select="0"/>
  <xsl:apply-templates select="(partinfo/title|info/title|docinfo/title|title)[1]"
                       mode="title.markup">
    <xsl:with-param name="allow-anchors" select="$allow-anchors"/>
  </xsl:apply-templates>
</xsl:template>
<!-- division -->
<xsl:template match="subpart">
  <!--<div class="{name(.)}">
    <xsl:call-template name="language.attribute"/>-->
    <xsl:if test="$generate.id.attributes != 0">
      <xsl:attribute name="id">
        <xsl:call-template name="object.id"/>
      </xsl:attribute>
    </xsl:if>

    <xsl:call-template name="part.titlepage"/>

    <xsl:variable name="toc.params">
      <xsl:call-template name="find.path.params">
        <xsl:with-param name="table" select="normalize-space($generate.toc)"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:if test="not(partintro) and contains($toc.params, 'toc')">
      <xsl:call-template name="division.toc"/>
    </xsl:if>
    
    <xsl:apply-templates/>
  <!--</div>-->
</xsl:template>

<xsl:template match="subpart" mode="make.part.toc">
  <xsl:call-template name="division.toc"/>
</xsl:template>

<xsl:template match="subpart/docinfo"></xsl:template>
<xsl:template match="subpart/partinfo"></xsl:template>
<xsl:template match="subpart/title"></xsl:template>
<xsl:template match="subpart/titleabbrev"></xsl:template>
<xsl:template match="subpart/subtitle"></xsl:template>

<xsl:template match="subpart" mode="division.number">
  <xsl:number from="book" count="subpart" format="I."/>
</xsl:template>

</xsl:stylesheet>