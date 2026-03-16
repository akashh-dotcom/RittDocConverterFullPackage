<?xml version='1.0'?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version='1.0'>
<!-- common.ritt.xsl -->
<!-- common overlays -->
<xsl:template name="section.level">
  <xsl:param name="node" select="."/>
  <xsl:choose>
    <xsl:when test="name($node)='sect1'">1</xsl:when>
    <xsl:when test="name($node)='sect2'">2</xsl:when>
    <xsl:when test="name($node)='sect3'">3</xsl:when>
    <xsl:when test="name($node)='sect4'">4</xsl:when>
    <xsl:when test="name($node)='sect5'">5</xsl:when>
    <xsl:when test="name($node)='sect6'">6</xsl:when>
    <xsl:when test="name($node)='sect7'">7</xsl:when>
    <xsl:when test="name($node)='sect8'">8</xsl:when>
    <xsl:when test="name($node)='sect9'">9</xsl:when>
    <xsl:when test="name($node)='sect10'">10</xsl:when>
    <xsl:when test="name($node)='section'">
      <xsl:choose>
        <xsl:when test="$node/../../../../../../section">6</xsl:when>
        <xsl:when test="$node/../../../../../section">5</xsl:when>
        <xsl:when test="$node/../../../../section">4</xsl:when>
        <xsl:when test="$node/../../../section">3</xsl:when>
        <xsl:when test="$node/../../section">2</xsl:when>
        <xsl:otherwise>1</xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:when test="name($node)='refsect1' or
                    name($node)='refsect2' or
                    name($node)='refsect3' or
                    name($node)='refsection' or
                    name($node)='refsynopsisdiv'">
      <xsl:call-template name="refentry.section.level">
        <xsl:with-param name="node" select="$node"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="name($node)='simplesect'">
      <xsl:choose>
        <xsl:when test="$node/../../sect1">2</xsl:when>
        <xsl:when test="$node/../../sect2">3</xsl:when>
        <xsl:when test="$node/../../sect3">4</xsl:when>
        <xsl:when test="$node/../../sect4">5</xsl:when>
        <xsl:when test="$node/../../sect5">5</xsl:when>
        <xsl:when test="$node/../../section">
          <xsl:choose>
            <xsl:when test="$node/../../../../../section">5</xsl:when>
            <xsl:when test="$node/../../../../section">4</xsl:when>
            <xsl:when test="$node/../../../section">3</xsl:when>
            <xsl:otherwise>2</xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>1</xsl:otherwise>
      </xsl:choose>
    </xsl:when>
   <xsl:when test="contains(name($node), 'sect')">5</xsl:when>
    <xsl:otherwise>1</xsl:otherwise>
  </xsl:choose>
</xsl:template><!-- section.level -->

<xsl:template name="person.name.first-last">
  <xsl:param name="node" select="."/>
  <xsl:if test="$node//honorific">
    <xsl:apply-templates select="$node//honorific[1]"/>
    <xsl:value-of select="$punct.honorific"/>
  </xsl:if>

<!-- note I am looping firstnames middle and lastnames in a group as the taging is not always 100 pct accurate and multiples do occur -->
<!-- 04-04-2006 added "$node//contrib"
				To appear "contrib" inside authorgroup -->
<xsl:for-each select="$node//firstname | $node//othername | $node//surname | $node//contrib">
	<xsl:choose>
		<xsl:when test="substring(normalize-space(.),1,1) = ','"></xsl:when>
		<xsl:otherwise><xsl:text> </xsl:text></xsl:otherwise>
	</xsl:choose>	
<!-- ## ORI <xsl:value-of select="normalize-space(.)"/>-->
<!-- ## 02/12/05 For deleting superscript content which is added in tirlepage.ritt.xsl -->


  <!-- ## Tease out the superscript digits from the surname -->
  <xsl:for-each select=".//text()">
      <xsl:choose>

        <xsl:when test="name(..) = 'superscript'">
          <xsl:choose>
            <xsl:when test="(ancestor::*[name(.) = 'surname'])"/>
            <xsl:otherwise>
              <xsl:value-of select="normalize-space(.)" />              
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="name(..) = 'emphasis'">
          <xsl:choose>
            <xsl:when test="(ancestor::*[name(.) = 'superscript'])"/>
            <xsl:otherwise>
              <xsl:value-of select="normalize-space(.)" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="normalize-space(.)" />
        </xsl:otherwise>

    </xsl:choose>
  </xsl:for-each>
</xsl:for-each>

<!-- 
  <xsl:if test="$node//othername and $author.othername.in.middle != 0">
    <xsl:if test="$node//honorific or $node//firstname">
      <xsl:text> </xsl:text>
    </xsl:if>
    <xsl:apply-templates select="$node//othername[1]"/>
  </xsl:if>

  <xsl:if test="$node//surname">
    <xsl:if test="$node//honorific or $node//firstname
                  or ($node//othername and $author.othername.in.middle != 0)">
      <xsl:text> </xsl:text>
    </xsl:if>
    <xsl:value-of select="normalize-space($node//surname[1])"/></xsl:if> -->

  <xsl:if test="$node//lineage"><!-- ## 28/11/05 Insert comma within lineage & authors only when lineage dont have comma ## 11/01/06 updated again ## --><xsl:if test="substring(normalize-space(.//lineage), 1, 1) != ','"><xsl:text>, </xsl:text></xsl:if>
  <!-- <xsl:apply-templates select="$node//lineage"/> -->
  <xsl:value-of select="normalize-space($node//lineage)"/></xsl:if>
  <xsl:if test="$node//degree">
  	<xsl:choose>
		<xsl:when test="substring-after($node//degree[1], ',')  != '' ">
		<!-- ## 26/04/06 insert comma before degree if not present -->
		<xsl:if test="substring(normalize-space($node//degree),1,1) != ','"><xsl:text>, </xsl:text></xsl:if>
		<!-- ## -->
		<xsl:value-of select="normalize-space($node//degree)"/></xsl:when>
		<xsl:otherwise><xsl:text>, </xsl:text><xsl:value-of select="normalize-space($node//degree)"/><!--<xsl:apply-templates select="$node//degree"/>--></xsl:otherwise>	
	</xsl:choose></xsl:if>
	<!-- ## 26/06/06 to view affiliation text -->
	<xsl:if test="$node//affiliation"><xsl:value-of select=".//affiliation"/></xsl:if>
</xsl:template>


<!-- ## 14/12/05 New Match added for last author in Info tag for removing superscript in text which is applied in titlepage.ritt.xsl same date --> 
<xsl:template name="lastautsup">
	<xsl:param name="authorname"><xsl:value-of select="."/></xsl:param>
	<xsl:param name="node" select="."/>
  <xsl:if test="$node//honorific">
    <xsl:apply-templates select="$node//honorific[1]"/>
    <xsl:value-of select="$punct.honorific"/>
  </xsl:if>
	<xsl:for-each select="$node//firstname | $node//othername | $node//surname">
		<xsl:choose>
			<xsl:when test="substring(normalize-space(.),1,1) = ','"></xsl:when>
			<xsl:otherwise><xsl:text> </xsl:text></xsl:otherwise>
		</xsl:choose>	
		<xsl:for-each select=".//text()">
			<xsl:choose>
				<xsl:when test="name(.) = 'superscript'"/>
				<xsl:when test="parent::*[name(.) = 'superscript']"/>
				<xsl:otherwise><xsl:value-of select="translate(normalize-space(.),',','')" /></xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:for-each>
	<xsl:if test="$node//lineage"><xsl:if test="substring(normalize-space(self::*[1]),number(string-length(normalize-space(self::*[1]))),1) != ','"><xsl:text>, </xsl:text></xsl:if><xsl:value-of select="normalize-space($node//lineage)"/></xsl:if>
	<xsl:if test="$node//degree">
	<xsl:choose>
		<xsl:when test="substring-after($node//degree[1], ',')  != '' "><xsl:value-of select="normalize-space($node//degree)"/></xsl:when>
		<xsl:otherwise><xsl:text>, </xsl:text><xsl:value-of select="normalize-space($node//degree)"/></xsl:otherwise>	
	</xsl:choose></xsl:if>
</xsl:template>


</xsl:stylesheet>
