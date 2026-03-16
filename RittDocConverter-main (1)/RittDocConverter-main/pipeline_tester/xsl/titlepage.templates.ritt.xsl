<?xml version='1.0'?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version='1.0'>
<!-- titlepage.templates.ritt.xsl -->
<!-- extentions for section titlepage.templates -->

<!-- templates for sect5 -->
<xsl:template match="title" mode="sect5.titlepage.recto.auto.mode">
  <xsl:apply-templates select="." mode="sect5.titlepage.recto.mode"/>
</xsl:template> 

<!-- templates for sect6 -->
<xsl:template name="sect6.titlepage.recto">
  <xsl:choose>
    <xsl:when test="sect6info/title">
      <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="sect6info/title"/>
    </xsl:when>
    <xsl:when test="info/title">
      <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="info/title"/>
    </xsl:when>
    <xsl:when test="title">
      <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="title"/>
    </xsl:when>
  </xsl:choose>

  <xsl:choose>
    <xsl:when test="sect6info/subtitle">
      <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="sect6info/subtitle"/>
    </xsl:when>
    <xsl:when test="info/subtitle">
      <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="info/subtitle"/>
    </xsl:when>
    <xsl:when test="subtitle">
      <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="subtitle"/>
    </xsl:when>
  </xsl:choose>

  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="sect6info/corpauthor"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="info/corpauthor"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="sect6info/authorgroup"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="info/authorgroup"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="sect6info/author"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="info/author"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="sect6info/othercredit"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="info/othercredit"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="sect6info/releaseinfo"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="info/releaseinfo"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="sect6info/copyright"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="info/copyright"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="sect6info/legalnotice"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="info/legalnotice"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="sect6info/pubdate"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="info/pubdate"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="sect6info/revision"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="info/revision"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="sect6info/revhistory"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="info/revhistory"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="sect6info/abstract"/>
  <xsl:apply-templates mode="sect6.titlepage.recto.auto.mode" select="info/abstract"/>
</xsl:template>

<xsl:template name="sect6.titlepage.verso">
</xsl:template>

<xsl:template name="sect6.titlepage.separator">
</xsl:template>

<xsl:template name="sect6.titlepage.before.recto">
</xsl:template>

<xsl:template name="sect6.titlepage.before.verso">
</xsl:template>

<xsl:template name="sect6.titlepage">
  <!--<div class="titlepage">
    <div>-->
    <xsl:call-template name="sect6.titlepage.before.recto"/>
    <xsl:call-template name="sect6.titlepage.recto"/>
    <!--</div>
    <div>-->
    <xsl:call-template name="sect6.titlepage.before.verso"/>
    <xsl:call-template name="sect6.titlepage.verso"/>
    <!--</div>-->
    <xsl:call-template name="sect6.titlepage.separator"/>
  <!--</div>-->
</xsl:template>

<xsl:template match="*" mode="sect6.titlepage.recto.mode">
  <!-- if an element isn't found in this mode, -->
  <!-- try the generic titlepage.mode -->
  <xsl:apply-templates select="." mode="titlepage.mode"/>
</xsl:template>

<xsl:template match="*" mode="sect6.titlepage.verso.mode">
  <!-- if an element isn't found in this mode, -->
  <!-- try the generic titlepage.mode -->
  <xsl:apply-templates select="." mode="titlepage.mode"/>
</xsl:template>

<xsl:template match="title" mode="sect6.titlepage.recto.auto.mode">
<!--<div xsl:use-attribute-sets="sect6.titlepage.recto.style">-->
<xsl:apply-templates select="." mode="sect6.titlepage.recto.mode"/>
<!--</div>-->
</xsl:template>

<xsl:template match="subtitle" mode="sect6.titlepage.recto.auto.mode">
<!--<div xsl:use-attribute-sets="sect6.titlepage.recto.style">-->
<xsl:apply-templates select="." mode="sect6.titlepage.recto.mode"/>
<!--</div>-->
</xsl:template>

<xsl:template match="corpauthor" mode="sect6.titlepage.recto.auto.mode">
<!--<div xsl:use-attribute-sets="sect6.titlepage.recto.style">-->
<xsl:apply-templates select="." mode="sect6.titlepage.recto.mode"/>
<!--</div>-->
</xsl:template>

<xsl:template match="authorgroup" mode="sect6.titlepage.recto.auto.mode">
<!--<div xsl:use-attribute-sets="sect6.titlepage.recto.style">-->
<xsl:apply-templates select="." mode="sect6.titlepage.recto.mode"/>
<!--</div>-->
</xsl:template>

<xsl:template match="author" mode="sect6.titlepage.recto.auto.mode">
<!--<div xsl:use-attribute-sets="sect6.titlepage.recto.style">-->
<xsl:apply-templates select="." mode="sect6.titlepage.recto.mode"/>
<!--</div>-->
</xsl:template>

<xsl:template match="othercredit" mode="sect6.titlepage.recto.auto.mode">
<!--<div xsl:use-attribute-sets="sect6.titlepage.recto.style">-->
<xsl:apply-templates select="." mode="sect6.titlepage.recto.mode"/>
<!--</div>-->
</xsl:template>

<xsl:template match="releaseinfo" mode="sect6.titlepage.recto.auto.mode">
<!--<div xsl:use-attribute-sets="sect6.titlepage.recto.style">-->
<xsl:apply-templates select="." mode="sect6.titlepage.recto.mode"/>
<!--</div>-->
</xsl:template>

<xsl:template match="copyright" mode="sect6.titlepage.recto.auto.mode">
<!--<div xsl:use-attribute-sets="sect6.titlepage.recto.style">-->
<xsl:apply-templates select="." mode="sect6.titlepage.recto.mode"/>
<!--</div>-->
</xsl:template>

<xsl:template match="legalnotice" mode="sect6.titlepage.recto.auto.mode">
<!--<div xsl:use-attribute-sets="sect6.titlepage.recto.style">-->
<xsl:apply-templates select="." mode="sect6.titlepage.recto.mode"/>
<!--</div>-->
</xsl:template>

<xsl:template match="pubdate" mode="sect6.titlepage.recto.auto.mode">
<!--<div xsl:use-attribute-sets="sect6.titlepage.recto.style">-->
<xsl:apply-templates select="." mode="sect6.titlepage.recto.mode"/>
<!--</div>-->
</xsl:template>

<xsl:template match="revision" mode="sect6.titlepage.recto.auto.mode">
<!--<div xsl:use-attribute-sets="sect6.titlepage.recto.style">-->
<xsl:apply-templates select="." mode="sect6.titlepage.recto.mode"/>
<!--</div>-->
</xsl:template>

<xsl:template match="revhistory" mode="sect6.titlepage.recto.auto.mode">
<!--<div xsl:use-attribute-sets="sect6.titlepage.recto.style">-->
<xsl:apply-templates select="." mode="sect6.titlepage.recto.mode"/>
<!--</div>-->
</xsl:template>

<xsl:template match="abstract" mode="sect6.titlepage.recto.auto.mode">
<!--<div xsl:use-attribute-sets="sect6.titlepage.recto.style">-->
<xsl:apply-templates select="." mode="sect6.titlepage.recto.mode"/>
<!--</div>-->
</xsl:template>

<!-- templates for sect7 -->
<xsl:template name="sect7.titlepage.recto">
  <xsl:choose>
    <xsl:when test="sect7info/title">
      <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="sect7info/title"/>
    </xsl:when>
    <xsl:when test="info/title">
      <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="info/title"/>
    </xsl:when>
    <xsl:when test="title">
      <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="title"/>
    </xsl:when>
  </xsl:choose>

  <xsl:choose>
    <xsl:when test="sect7info/subtitle">
      <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="sect7info/subtitle"/>
    </xsl:when>
    <xsl:when test="info/subtitle">
      <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="info/subtitle"/>
    </xsl:when>
    <xsl:when test="subtitle">
      <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="subtitle"/>
    </xsl:when>
  </xsl:choose>

  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="sect7info/corpauthor"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="info/corpauthor"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="sect7info/authorgroup"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="info/authorgroup"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="sect7info/author"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="info/author"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="sect7info/othercredit"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="info/othercredit"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="sect7info/releaseinfo"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="info/releaseinfo"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="sect7info/copyright"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="info/copyright"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="sect7info/legalnotice"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="info/legalnotice"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="sect7info/pubdate"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="info/pubdate"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="sect7info/revision"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="info/revision"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="sect7info/revhistory"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="info/revhistory"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="sect7info/abstract"/>
  <xsl:apply-templates mode="sect7.titlepage.recto.auto.mode" select="info/abstract"/>
</xsl:template>

<xsl:template name="sect7.titlepage.verso">
</xsl:template>

<xsl:template name="sect7.titlepage.separator">
</xsl:template>

<xsl:template name="sect7.titlepage.before.recto">
</xsl:template>

<xsl:template name="sect7.titlepage.before.verso">
</xsl:template>

<xsl:template name="sect7.titlepage">
  <!--<div class="titlepage">
    <div>-->
    <xsl:call-template name="sect7.titlepage.before.recto"/>
    <xsl:call-template name="sect7.titlepage.recto"/>
    <!--</div>
    <div>-->
    <xsl:call-template name="sect7.titlepage.before.verso"/>
    <xsl:call-template name="sect7.titlepage.verso"/>
    <!--</div>-->
    <xsl:call-template name="sect7.titlepage.separator"/>
  <!--</div>-->
</xsl:template>

<xsl:template match="*" mode="sect7.titlepage.recto.mode">
  <!-- if an element isn't found in this mode, -->
  <!-- try the generic titlepage.mode -->
  <xsl:apply-templates select="." mode="titlepage.mode"/>
</xsl:template>

<xsl:template match="*" mode="sect7.titlepage.verso.mode">
  <!-- if an element isn't found in this mode, -->
  <!-- try the generic titlepage.mode -->
  <xsl:apply-templates select="." mode="titlepage.mode"/>
</xsl:template>

<xsl:template match="title" mode="sect7.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect7.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect7.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="subtitle" mode="sect7.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect7.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect7.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="corpauthor" mode="sect7.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect7.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect7.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="authorgroup" mode="sect7.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect7.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect7.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="author" mode="sect7.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect7.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect7.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="othercredit" mode="sect7.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect7.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect7.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="releaseinfo" mode="sect7.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect7.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect7.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="copyright" mode="sect7.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect7.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect7.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="legalnotice" mode="sect7.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect7.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect7.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="pubdate" mode="sect7.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect7.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect7.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="revision" mode="sect7.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect7.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect7.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="revhistory" mode="sect7.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect7.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect7.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="abstract" mode="sect7.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect7.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect7.titlepage.recto.mode"/>
</div>
</xsl:template>

<!-- templates for sect8 -->
<xsl:template name="sect8.titlepage.recto">
  <xsl:choose>
    <xsl:when test="sect8info/title">
      <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="sect8info/title"/>
    </xsl:when>
    <xsl:when test="info/title">
      <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="info/title"/>
    </xsl:when>
    <xsl:when test="title">
      <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="title"/>
    </xsl:when>
  </xsl:choose>

  <xsl:choose>
    <xsl:when test="sect8info/subtitle">
      <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="sect8info/subtitle"/>
    </xsl:when>
    <xsl:when test="info/subtitle">
      <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="info/subtitle"/>
    </xsl:when>
    <xsl:when test="subtitle">
      <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="subtitle"/>
    </xsl:when>
  </xsl:choose>

  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="sect8info/corpauthor"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="info/corpauthor"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="sect8info/authorgroup"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="info/authorgroup"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="sect8info/author"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="info/author"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="sect8info/othercredit"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="info/othercredit"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="sect8info/releaseinfo"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="info/releaseinfo"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="sect8info/copyright"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="info/copyright"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="sect8info/legalnotice"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="info/legalnotice"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="sect8info/pubdate"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="info/pubdate"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="sect8info/revision"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="info/revision"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="sect8info/revhistory"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="info/revhistory"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="sect8info/abstract"/>
  <xsl:apply-templates mode="sect8.titlepage.recto.auto.mode" select="info/abstract"/>
</xsl:template>

<xsl:template name="sect8.titlepage.verso">
</xsl:template>

<xsl:template name="sect8.titlepage.separator">
</xsl:template>

<xsl:template name="sect8.titlepage.before.recto">
</xsl:template>

<xsl:template name="sect8.titlepage.before.verso">
</xsl:template>

<xsl:template name="sect8.titlepage">
  <!--<div class="titlepage">
    <div>-->
    <xsl:call-template name="sect8.titlepage.before.recto"/>
    <xsl:call-template name="sect8.titlepage.recto"/>
    <!--</div>
    <div>-->
    <xsl:call-template name="sect8.titlepage.before.verso"/>
    <xsl:call-template name="sect8.titlepage.verso"/>
    <!--</div>-->
    <xsl:call-template name="sect8.titlepage.separator"/>
  <!--</div>-->
</xsl:template>

<xsl:template match="*" mode="sect8.titlepage.recto.mode">
  <!-- if an element isn't found in this mode, -->
  <!-- try the generic titlepage.mode -->
  <xsl:apply-templates select="." mode="titlepage.mode"/>
</xsl:template>

<xsl:template match="*" mode="sect8.titlepage.verso.mode">
  <!-- if an element isn't found in this mode, -->
  <!-- try the generic titlepage.mode -->
  <xsl:apply-templates select="." mode="titlepage.mode"/>
</xsl:template>

<xsl:template match="title" mode="sect8.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect8.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect8.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="subtitle" mode="sect8.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect8.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect8.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="corpauthor" mode="sect8.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect8.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect8.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="authorgroup" mode="sect8.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect8.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect8.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="author" mode="sect8.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect8.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect8.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="othercredit" mode="sect8.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect8.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect8.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="releaseinfo" mode="sect8.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect8.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect8.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="copyright" mode="sect8.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect8.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect8.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="legalnotice" mode="sect8.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect8.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect8.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="pubdate" mode="sect8.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect8.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect8.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="revision" mode="sect8.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect8.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect8.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="revhistory" mode="sect8.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect8.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect8.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="abstract" mode="sect8.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect8.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect8.titlepage.recto.mode"/>
</div>
</xsl:template>

<!-- templates for sect9 -->
<xsl:template name="sect9.titlepage.recto">
  <xsl:choose>
    <xsl:when test="sect9info/title">
      <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="sect9info/title"/>
    </xsl:when>
    <xsl:when test="info/title">
      <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="info/title"/>
    </xsl:when>
    <xsl:when test="title">
      <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="title"/>
    </xsl:when>
  </xsl:choose>

  <xsl:choose>
    <xsl:when test="sect9info/subtitle">
      <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="sect9info/subtitle"/>
    </xsl:when>
    <xsl:when test="info/subtitle">
      <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="info/subtitle"/>
    </xsl:when>
    <xsl:when test="subtitle">
      <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="subtitle"/>
    </xsl:when>
  </xsl:choose>

  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="sect9info/corpauthor"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="info/corpauthor"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="sect9info/authorgroup"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="info/authorgroup"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="sect9info/author"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="info/author"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="sect9info/othercredit"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="info/othercredit"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="sect9info/releaseinfo"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="info/releaseinfo"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="sect9info/copyright"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="info/copyright"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="sect9info/legalnotice"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="info/legalnotice"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="sect9info/pubdate"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="info/pubdate"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="sect9info/revision"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="info/revision"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="sect9info/revhistory"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="info/revhistory"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="sect9info/abstract"/>
  <xsl:apply-templates mode="sect9.titlepage.recto.auto.mode" select="info/abstract"/>
</xsl:template>

<xsl:template name="sect9.titlepage.verso">
</xsl:template>

<xsl:template name="sect9.titlepage.separator">
</xsl:template>

<xsl:template name="sect9.titlepage.before.recto">
</xsl:template>

<xsl:template name="sect9.titlepage.before.verso">
</xsl:template>

<xsl:template name="sect9.titlepage">
  <!--<div class="titlepage">
    <div>-->
    <xsl:call-template name="sect9.titlepage.before.recto"/>
    <xsl:call-template name="sect9.titlepage.recto"/>
    <!--</div>
    <div>-->
    <xsl:call-template name="sect9.titlepage.before.verso"/>
    <xsl:call-template name="sect9.titlepage.verso"/>
    <!--</div>-->
    <xsl:call-template name="sect9.titlepage.separator"/>
  <!--</div>-->
</xsl:template>

<xsl:template match="*" mode="sect9.titlepage.recto.mode">
  <!-- if an element isn't found in this mode, -->
  <!-- try the generic titlepage.mode -->
  <xsl:apply-templates select="." mode="titlepage.mode"/>
</xsl:template>

<xsl:template match="*" mode="sect9.titlepage.verso.mode">
  <!-- if an element isn't found in this mode, -->
  <!-- try the generic titlepage.mode -->
  <xsl:apply-templates select="." mode="titlepage.mode"/>
</xsl:template>

<xsl:template match="title" mode="sect9.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect9.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect9.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="subtitle" mode="sect9.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect9.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect9.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="corpauthor" mode="sect9.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect9.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect9.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="authorgroup" mode="sect9.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect9.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect9.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="author" mode="sect9.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect9.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect9.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="othercredit" mode="sect9.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect9.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect9.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="releaseinfo" mode="sect9.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect9.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect9.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="copyright" mode="sect9.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect9.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect9.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="legalnotice" mode="sect9.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect9.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect9.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="pubdate" mode="sect9.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect9.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect9.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="revision" mode="sect9.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect9.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect9.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="revhistory" mode="sect9.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect9.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect9.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="abstract" mode="sect9.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect9.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect9.titlepage.recto.mode"/>
</div>
</xsl:template>

<!-- templates for sect10 -->
<xsl:template name="sect10.titlepage.recto">
  <xsl:choose>
    <xsl:when test="sect10info/title">
      <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="sect10info/title"/>
    </xsl:when>
    <xsl:when test="info/title">
      <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="info/title"/>
    </xsl:when>
    <xsl:when test="title">
      <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="title"/>
    </xsl:when>
  </xsl:choose>

  <xsl:choose>
    <xsl:when test="sect10info/subtitle">
      <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="sect10info/subtitle"/>
    </xsl:when>
    <xsl:when test="info/subtitle">
      <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="info/subtitle"/>
    </xsl:when>
    <xsl:when test="subtitle">
      <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="subtitle"/>
    </xsl:when>
  </xsl:choose>

  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="sect10info/corpauthor"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="info/corpauthor"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="sect10info/authorgroup"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="info/authorgroup"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="sect10info/author"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="info/author"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="sect10info/othercredit"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="info/othercredit"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="sect10info/releaseinfo"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="info/releaseinfo"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="sect10info/copyright"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="info/copyright"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="sect10info/legalnotice"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="info/legalnotice"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="sect10info/pubdate"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="info/pubdate"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="sect10info/revision"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="info/revision"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="sect10info/revhistory"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="info/revhistory"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="sect10info/abstract"/>
  <xsl:apply-templates mode="sect10.titlepage.recto.auto.mode" select="info/abstract"/>
</xsl:template>

<xsl:template name="sect10.titlepage.verso">
</xsl:template>

<xsl:template name="sect10.titlepage.separator">
</xsl:template>

<xsl:template name="sect10.titlepage.before.recto">
</xsl:template>

<xsl:template name="sect10.titlepage.before.verso">
</xsl:template>

<xsl:template name="sect10.titlepage">
  <!--<div class="titlepage">
    <div>-->
    <xsl:call-template name="sect10.titlepage.before.recto"/>
    <xsl:call-template name="sect10.titlepage.recto"/>
    <!--</div>
    <div>-->
    <xsl:call-template name="sect10.titlepage.before.verso"/>
    <xsl:call-template name="sect10.titlepage.verso"/>
    <!--</div>-->
    <xsl:call-template name="sect10.titlepage.separator"/>
  <!--</div>-->
</xsl:template>

<xsl:template match="*" mode="sect10.titlepage.recto.mode">
  <!-- if an element isn't found in this mode, -->
  <!-- try the generic titlepage.mode -->
  <xsl:apply-templates select="." mode="titlepage.mode"/>
</xsl:template>

<xsl:template match="*" mode="sect10.titlepage.verso.mode">
  <!-- if an element isn't found in this mode, -->
  <!-- try the generic titlepage.mode -->
  <xsl:apply-templates select="." mode="titlepage.mode"/>
</xsl:template>

<xsl:template match="title" mode="sect10.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect10.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect10.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="subtitle" mode="sect10.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect10.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect10.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="corpauthor" mode="sect10.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect10.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect10.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="authorgroup" mode="sect10.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect10.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect10.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="author" mode="sect10.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect10.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect10.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="othercredit" mode="sect10.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect10.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect10.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="releaseinfo" mode="sect10.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect10.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect10.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="copyright" mode="sect10.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect10.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect10.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="legalnotice" mode="sect10.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect10.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect10.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="pubdate" mode="sect10.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect10.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect10.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="revision" mode="sect10.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect10.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect10.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="revhistory" mode="sect10.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect10.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect10.titlepage.recto.mode"/>
</div>
</xsl:template>

<xsl:template match="abstract" mode="sect10.titlepage.recto.auto.mode">
<div xsl:use-attribute-sets="sect10.titlepage.recto.style">
<xsl:apply-templates select="." mode="sect10.titlepage.recto.mode"/>
</div>
</xsl:template>

</xsl:stylesheet>