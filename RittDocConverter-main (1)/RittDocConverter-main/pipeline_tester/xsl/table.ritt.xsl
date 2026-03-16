<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:doc="http://nwalsh.com/xsl/documentation/1.0"
                xmlns:stbl="http://nwalsh.com/xslt/ext/com.nwalsh.saxon.Table"
                xmlns:xtbl="com.nwalsh.xalan.Table"
                xmlns:lxslt="http://xml.apache.org/xslt"
                xmlns:ptbl="http://nwalsh.com/xslt/ext/xsltproc/python/Table"
                exclude-result-prefixes="doc stbl xtbl lxslt ptbl"
                version='1.0'>

<xsl:template match="table|figure|equation">
	<xsl:choose>
		<xsl:when test="literallayout">
      <xsl:apply-templates select="literallayout|mediaobject"></xsl:apply-templates>
		</xsl:when>
		<xsl:when test="tgroup">
		  <xsl:call-template name="notitle.formal.object" /> 
		</xsl:when>		
	</xsl:choose>
  <xsl:if test="mediaobject/caption">
    <xsl:apply-templates select="mediaobject/caption" />
  </xsl:if>
  <xsl:if test="printOrEmail = 0">
    <xsl:call-template name="process.footnotes" />
  </xsl:if>
</xsl:template>

<xsl:template match="table/title|entrytbl/title" name="table.title">
	<xsl:param name="caption" select="1"/>
	<xsl:if test="$caption = 1">
		<caption>
		  <!--Squish #64 -->
			<!--<xsl:choose>
				<xsl:when test="./emphasis">-->
					<xsl:apply-templates />
				<!--</xsl:when>
			</xsl:choose>-->
		</caption>	
	</xsl:if>
</xsl:template>
  
<xsl:template match="table/title/emphasis|entrytbl/title/emphasis" name="table.title.emphasis">
  <xsl:choose>
    <xsl:when test="@role = 'strong'">
      <strong>
        <xsl:apply-templates />
      </strong>
    </xsl:when>
		<xsl:otherwise>
			<em>
				<xsl:apply-templates />
			</em>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template match="table/title/emphasis/superscript|entrytbl/title/emphasis/superscript" name="table.title.emphasis.superscript">
  <sup>
    <xsl:apply-templates />
  </sup>
</xsl:template>

<!-- ==================================================================== -->
<xsl:template match="tgroup" name="tgroup2">
  <table>
		<xsl:copy-of select="../@id"/> 
		<xsl:if test="../@freezeheadercol = 'true'">
			<xsl:attribute name="data-freeze-header-col">true</xsl:attribute>
		</xsl:if>
		<xsl:if test="../@freezeheaderrow = 'true'">
			<xsl:attribute name="data-freeze-header-row">true</xsl:attribute>
		</xsl:if>

    <xsl:choose>
      <!-- If this is not the first tgroup in the current table, skip this block -->
      <xsl:when test="count(preceding-sibling::tgroup) > 0">
      </xsl:when>

      <!-- If there's a textobject/phrase for the table summary, use it -->
      <xsl:when test="../textobject/phrase">
        <xsl:attribute name="summary">
          <xsl:value-of select="../textobject/phrase"/>
        </xsl:attribute>
      </xsl:when>

      <xsl:when test="../title">
        <xsl:if test="string-length(../title) > 0">
          <xsl:apply-templates select="../title" />
					<!--<xsl:value-of select="string(../title)"/>-->
        </xsl:if>
      </xsl:when>
    </xsl:choose>

    <xsl:variable name="explicit.table.width">
      <xsl:call-template name="dbhtml-attribute">
        <xsl:with-param name="pis" select="../processing-instruction('dbhtml')[1]"/>
        <xsl:with-param name="attribute" select="'table-width'"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="table.width">
      <xsl:choose>
        <!-- 18-10-06 to reduce table width which appears inside listitem -->
        <xsl:when test="ancestor::listitem">
          <!-- <xsl:value-of select="100 - (count(ancestor::listitem) * 3)"/>%--><!-- 18-09-07 to keep table width which appears inside listitem  as 100% -->100%
        </xsl:when>
        <xsl:when test="$explicit.table.width != ''">
          <xsl:value-of select="$explicit.table.width"/>
        </xsl:when>
        <xsl:when test="$default.table.width = ''">
          <xsl:text>100%</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$default.table.width"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="$default.table.width != '' or $explicit.table.width != ''">
      <xsl:attribute name="width">
        <xsl:choose>
          <xsl:when test="contains($table.width, '%')">
            <xsl:value-of select="$table.width"/>
          </xsl:when>
          <xsl:when test="$use.extensions != 0 and $tablecolumns.extension != 0">
            <xsl:choose>
              <xsl:when test="function-available('stbl:convertLength')">
                <xsl:value-of select="stbl:convertLength($table.width)"/>
              </xsl:when>
              <xsl:when test="function-available('xtbl:convertLength')">
                <xsl:value-of select="xtbl:convertLength($table.width)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:message terminate="yes">
                  <xsl:text>No convertLength function available.</xsl:text>
                </xsl:message>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$table.width"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
    </xsl:if>

    <xsl:apply-templates select="thead"/>
    <xsl:choose>
			<xsl:when test="tfoot"><xsl:apply-templates select="tfoot" mode="tgroup2" /></xsl:when>
			<xsl:otherwise>
					<xsl:if test="..//footnote">
					  <tfoot>
							<xsl:call-template name="ritt.footnote.handler">
								<xsl:with-param name="cols" select="@cols" />
							</xsl:call-template>		
					  </tfoot>
					</xsl:if>
			</xsl:otherwise>	
	</xsl:choose>	
    
  <xsl:apply-templates select="tbody"/>

  </table>
</xsl:template>

<xsl:template match="tfoot" mode="tgroup2">
  <xsl:element name="{name(.)}">

    <xsl:if test="@align">
      <xsl:attribute name="align">
        <xsl:value-of select="@align"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="@char">
      <xsl:attribute name="char">
        <xsl:value-of select="@char"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="@charoff">
      <xsl:attribute name="charoff">
        <xsl:value-of select="@charoff"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="@valign">
      <xsl:attribute name="valign">
        <xsl:value-of select="@valign"/>
      </xsl:attribute>
    </xsl:if>
	
    <xsl:apply-templates select="row[1]">
      <xsl:with-param name="spans">
        <xsl:call-template name="blank.spans">
          <xsl:with-param name="cols" select="../@cols"/>
        </xsl:call-template>
      </xsl:with-param>
    </xsl:apply-templates>

		<xsl:if test="name(.) = 'tfoot' ">		
				<xsl:call-template name="ritt.footnote.handler">
					<xsl:with-param name="cols" select="../@cols" />
				</xsl:call-template>		
		</xsl:if>	
  </xsl:element>
</xsl:template>

<xsl:template name="ritt.footnote.handler">
	<xsl:param name="cols" select="1" />	
	<xsl:if test="ancestor::table//footnote">
	    <tr>
        <th colspan="{$cols}">
					<div class="footnotes">
						<xsl:apply-templates select="ancestor::table//footnote[@label = '']" mode="table.footnote.mode" />
					</div>
					<div class="footnotes">
						<xsl:apply-templates select="ancestor::table//footnote[translate(@label, 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ', '') != '']" mode="table.footnote.mode"><xsl:sort select="@label"></xsl:sort></xsl:apply-templates>
          </div>
					<div class="footnotes">
						<xsl:apply-templates select="ancestor::table//footnote[translate(@label, 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ', '') = '']" mode="table.footnote.mode"><xsl:sort select="@label"></xsl:sort></xsl:apply-templates>
					</div>
				</th>	
		</tr>		    
    </xsl:if>
</xsl:template>		
</xsl:stylesheet>