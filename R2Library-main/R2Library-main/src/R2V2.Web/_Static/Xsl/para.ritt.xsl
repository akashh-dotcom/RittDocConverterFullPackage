<?xml version='1.0'?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version='1.0'
                exclude-result-prefixes="xsl"	>

<xsl:template name="paragraph">
  <xsl:param name="class" select="''"/>
  <xsl:param name="content"/>

	<xsl:choose>
		<xsl:when test="ancestor::*[name() = 'sect2']"></xsl:when>

<!-- ## 14/11/05 "When" Match added to remove duplicate boxes at sect1 level ## -->
		<xsl:when test="ancestor::*[name() = 'sect1']"></xsl:when>
		
		<xsl:otherwise>
			<xsl:apply-templates select="note|sidebar|important|warning|caution|tip" mode="anticipated" >
				<xsl:with-param name="posfilter" select="2" />	
			</xsl:apply-templates>
		</xsl:otherwise	>
	</xsl:choose>		

  <xsl:variable name="p">
    <p>
      <xsl:if test="$class != ''">
        <xsl:attribute name="class">
          <xsl:value-of select="$class"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:copy-of select="$content"/>
    </p>
  </xsl:variable>

  <xsl:choose>
    <xsl:when test="$html.cleanup != 0">
      <xsl:call-template name="unwrap.p">
        <xsl:with-param name="p" select="$p"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:otherwise>
      <xsl:copy-of select="$p"/>
    </xsl:otherwise>
  </xsl:choose>

	<xsl:choose>
		<xsl:when test="ancestor::blockquote or name(.) = 'para'"><xsl:apply-templates mode="delayed" select="descendant-or-self::table | descendant-or-self::figure | descendant-or-self::equation"	><xsl:with-param name="inline" select="2"	/></xsl:apply-templates></xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="delayed" select="descendant-or-self::table | descendant-or-self::figure | descendant-or-self::equation"/></xsl:otherwise>	
	</xsl:choose>		
</xsl:template>

<xsl:template match="formalpara/title">
  <xsl:variable name="titleStr1">
      <xsl:apply-templates/>
  </xsl:variable>  
  <xsl:variable name="titleStr"><xsl:value-of select="normalize-space($titleStr1)"/></xsl:variable>
  <xsl:variable name="lastChar">
    <xsl:if test="$titleStr != ''">
      <xsl:value-of select="substring($titleStr,string-length($titleStr),1)"/>
    </xsl:if>
  </xsl:variable>
  <strong>
    <xsl:copy-of select="$titleStr1"/>
    <xsl:if test="$lastChar != '' and not( contains($runinhead.title.end.punct, $lastChar))">
      <xsl:value-of select="$runinhead.default.title.end.punct"/>
    </xsl:if>
    <xsl:text>&#160;</xsl:text>
  </strong>
</xsl:template>


  
<xsl:template match="emphasis">
  <xsl:choose>
	  <xsl:when test="parent::biblioid[@otherclass='PubMedID']">
			<!-- don't process emphasis tags in PubMedID links -->
			<xsl:value-of select="."	/>	
    </xsl:when>
    <xsl:when test="@role = 'strong'">
      <strong>
        <xsl:call-template name="inline.emphasis" />
      </strong>
    </xsl:when>
    <xsl:when test="@role = 'underline'">
      <em class="underline">
		<xsl:call-template name="inline.emphasis" />
      </em>
    </xsl:when>
	<xsl:when test="@role = 'strike'">	<!-- render strike with span - output should not be italic by default -->
	  <span class="strike">
		<xsl:call-template name="inline.emphasis" />
	  </span>
	</xsl:when>
		<xsl:when test="@role = 'bg'">
			<span class="retain-bg">
				<xsl:call-template name="inline.emphasis" />
			</span>
		</xsl:when>
		<xsl:when test="@role = 'bi'">
			<strong><em>
					<xsl:call-template name="inline.emphasis" />
				</em></strong>
		</xsl:when>
    <xsl:when test="@role != ''">
			<span>
				<xsl:attribute name="style">
					color:<xsl:call-template name="emphasis.color" />
				</xsl:attribute>
				<xsl:call-template name="inline.emphasis" />
			</span>
		</xsl:when>
		<xsl:otherwise>
			<em>
				<xsl:call-template name="inline.emphasis" />
			</em>
		</xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template name="emphasis.color">
	<xsl:choose>
		<xsl:when test="@role = 'r'">red</xsl:when>
		<xsl:when test="@role = 'g'">green</xsl:when>
		<xsl:when test="@role = 'b'">blue</xsl:when>
		<xsl:when test="@role = 'y'">yellow</xsl:when>
		<xsl:otherwise><xsl:value-of select="@role" /></xsl:otherwise>
	</xsl:choose>
</xsl:template>


 <xsl:template match="blockquote/para">
   <xsl:choose>
     <xsl:when test="@role = 'right'">
       <cite>
         &#8212;<xsl:apply-templates/>
       </cite>
     </xsl:when>
     <xsl:otherwise>
			 <xsl:call-template name="paragraph">
				 <xsl:with-param name="content">
					 <xsl:apply-templates />
				 </xsl:with-param>
			 </xsl:call-template>
			 <xsl:apply-templates select="note|sidebar|important|warning|caution|tip" mode="anticipated" />
     </xsl:otherwise>
   </xsl:choose>
 </xsl:template>

</xsl:stylesheet>