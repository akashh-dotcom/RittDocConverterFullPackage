<?xml version='1.0'?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version='1.0'
                xmlns:str="http://xsltsl.org/string" 
                exclude-result-prefixes="str xsl">

<xsl:template match="ulink" name="ulink">
  <xsl:variable name="href" select="@url" />
  <xsl:variable name="link">
	  <xsl:choose>
			<!--<xsl:when test="$printOrEmail = 1"><xsl:apply-templates /></xsl:when>-->	
			<xsl:when test="@type='disease'"><xsl:call-template name="ulink.disease"	/></xsl:when>
			<xsl:when test="@type='drugsynonym' "><xsl:call-template name="ulink.drugsynonym"	/></xsl:when>
			<xsl:when test="@type='drug' "><xsl:call-template name="ulink.drug"	/></xsl:when>
			<xsl:when test="@type='keyword' "><xsl:call-template name="ulink.keyword"	/></xsl:when>
			<xsl:when test="@type='keywords' "><xsl:call-template name="ulink.keywords"	/></xsl:when>
		  <xsl:when test="@type='tabers' "><xsl:call-template name="ulink.tabers"	/></xsl:when>
			<xsl:otherwise>
        <a data-toggle="window">
          <xsl:if test="@id"><xsl:attribute name="name"><xsl:value-of select="@id"/></xsl:attribute></xsl:if>

          <xsl:attribute name="href">
		        <xsl:choose>
			        <xsl:when test="$href != ''"><xsl:value-of select="$href"/></xsl:when>
			        <xsl:otherwise><xsl:value-of select="@url"/></xsl:otherwise>	
            </xsl:choose>	
	        </xsl:attribute>
                    
          <xsl:attribute name="target">
            <xsl:choose>
              <xsl:when test="$ulink.target != ''">
                <xsl:value-of select="$ulink.target"/>
              </xsl:when>
              <xsl:otherwise>_blank</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>

          <xsl:attribute name="class">
            <xsl:if test="$ulink.target != ''">external</xsl:if>
          </xsl:attribute>

          <xsl:choose>
          <xsl:when test="count(child::node())=0">
            <xsl:value-of select="@url"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates/>
          </xsl:otherwise>
        </xsl:choose>
        </a>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:copy-of select="$link"/>
</xsl:template>

<xsl:template match="ulink/emphasis">
  <xsl:choose>
    <xsl:when test="@role = 'strong'">
      <strong>
        <xsl:apply-templates/>
      </strong>
    </xsl:when>
    <xsl:otherwise>
      <em>
        <xsl:apply-templates/>
      </em>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template name="ulink.drug" >
			<a>
				<xsl:if test="@id">
					<xsl:attribute name="name">
						<xsl:value-of select="@id"/>
					</xsl:attribute>
				</xsl:if>
    <xsl:attribute name="href"><xsl:value-of select="$baseUrl"	/>/search?q=<xsl:value-of select="." /></xsl:attribute>

				<xsl:choose>
					<xsl:when test="count(child::node())=0">
						<xsl:value-of select="@url"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates/>
					</xsl:otherwise>
				</xsl:choose>
			</a>
</xsl:template>	

  <xsl:template name="ulink.tabers" >
		<xsl:choose>
			<xsl:when test="not(parent::ulink[@type='tabers'])">
				<span>
					<xsl:attribute name="class">tabers-term</xsl:attribute>
					<xsl:attribute name="data-term-id">
						<xsl:value-of select="@termId" />
					</xsl:attribute>
					<!--<xsl:value-of select="."/>-->
					<xsl:apply-templates/>
				</span>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates/>
			</xsl:otherwise>
		</xsl:choose>
  </xsl:template>	

  <xsl:template name="ulink.disease" >
				<a>
					<xsl:if test="@id">
						<xsl:attribute name="name">
							<xsl:value-of select="@id"/>
						</xsl:attribute>
					</xsl:if>
      <xsl:attribute name="href"><xsl:value-of select="$baseUrl"	/>/search?q=<xsl:value-of select="." /></xsl:attribute>
					<xsl:choose>
						<xsl:when test="count(child::node())=0">
							<xsl:value-of select="@url"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates/>
						</xsl:otherwise>
					</xsl:choose>
				</a>
  </xsl:template>	

  <xsl:template name="ulink.keyword" >
				<a>
					<xsl:if test="@id">
						<xsl:attribute name="name">
							<xsl:value-of select="@id"/>
						</xsl:attribute>
					</xsl:if>
      <xsl:attribute name="href"><xsl:value-of select="$baseUrl"	/>/search?q=<xsl:value-of select="." /></xsl:attribute>
					<xsl:choose>
						<xsl:when test="count(child::node())=0">
							<xsl:value-of select="@url"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates/>
						</xsl:otherwise>
					</xsl:choose>
				</a>
  </xsl:template>	

  <xsl:template name="ulink.keywords" >
				<a>
					<xsl:if test="@id">
						<xsl:attribute name="name">
							<xsl:value-of select="@id"/>
						</xsl:attribute>
					</xsl:if>
      <xsl:attribute name="href"><xsl:value-of select="@url" /></xsl:attribute>
					<xsl:choose>
						<xsl:when test="count(child::node())=0">
							<xsl:value-of select="@url"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates/>
						</xsl:otherwise>
					</xsl:choose>
				</a>
  </xsl:template>	

  <xsl:template name="ulink.drugsynonym" >
				<a>
					<xsl:if test="@id">
						<xsl:attribute name="name">
							<xsl:value-of select="@id"/>
						</xsl:attribute>
					</xsl:if>
      <xsl:attribute name="href"><xsl:value-of select="$baseUrl"	/>/search?q=<xsl:value-of select="." /></xsl:attribute>
					<xsl:value-of select="." />
				</a>
  </xsl:template>	
</xsl:stylesheet>