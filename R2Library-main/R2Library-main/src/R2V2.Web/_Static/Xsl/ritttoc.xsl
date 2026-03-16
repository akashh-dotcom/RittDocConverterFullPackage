<?xml version='1.0'?>
<!DOCTYPE stylesheet [
	<!ENTITY nbsp "&#160;" >
	<!ENTITY lowercase "'abcdefghijklmnopqrstuvwxyz'">
	<!ENTITY uppercase "'ABCDEFGHIJKLMNOPQRSTUVWXYZ'">
] > 
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:exsl="http://exslt.org/common"
                exclude-result-prefixes="exsl xsl"
                version='1.0'>

<xsl:output method="html"
            encoding="ISO-8859-1"
            indent="no"/>
  
<xsl:param name="level">1</xsl:param>	
<xsl:param name="objectid"></xsl:param>
<xsl:param name="disablelinks"></xsl:param>
<xsl:param name="contentlinks"></xsl:param>
<xsl:param name="isbndir">
  <xsl:choose>
    <xsl:when test="//risinfo[1]/isbn">
      <xsl:value-of select="//risinfo[1]/isbn" />
    </xsl:when>
    <xsl:when test="//bookinfo[1]/isbn">
      <xsl:value-of select="//bookinfo[1]/isbn" />
    </xsl:when>
  </xsl:choose>
</xsl:param>
<xsl:param name="baseUrl" > </xsl:param>
<xsl:param name="email" select="0"	/>
  
<xsl:template match="/"> 
    <div class="accordion" data-implement="accordion">
      <xsl:if test="toc/tocfront != '' and $objectid = '' ">
        <h3 class="accordionhead closed">
          <xsl:choose>
			<xsl:when test="$disablelinks">
			  Front Matter
			</xsl:when>
            <xsl:when test="$contentlinks">
              <a><xsl:attribute name="href"><xsl:value-of select="$baseUrl"	/>/resource/title/<xsl:value-of select="$isbndir" /></xsl:attribute>Front Matter</a>
            </xsl:when>
            <xsl:otherwise>
              <a href="#turnaway" data-toggle="modal">Front Matter</a>
            </xsl:otherwise>
          </xsl:choose>
        </h3>
        <div class="accordioncontent">
          <ul>
            <xsl:apply-templates mode="ritt.toc.front" select="toc/tocfront"></xsl:apply-templates>
          </ul>
        </div>
      </xsl:if>
      <xsl:if test="$objectid = '' ">
        <xsl:apply-templates mode="ritt.toc" select="toc/node()" ></xsl:apply-templates>
      </xsl:if>
      <xsl:if test="$objectid != '' ">
        <xsl:apply-templates mode="ritt.toc" select="//tocchap[tocentry/@linkend = $objectid]" ></xsl:apply-templates>
      </xsl:if>
    </div>
  
</xsl:template>

<xsl:template match="title" mode="ritt.toc" ></xsl:template>

<xsl:template match="tocchap|tocpart|tocsubpart" mode="ritt.toc" >
  <h3 class="accordionhead closed">
    <xsl:choose>
	  <xsl:when test="$disablelinks">
		  <xsl:call-template name="toc.title.lookup" />
	  </xsl:when>
      <xsl:when test="$contentlinks">
	      <a><xsl:attribute name="id"><xsl:value-of select="tocentry/@linkend" /></xsl:attribute>
	      <xsl:attribute name="href">
	      <xsl:choose>
		      <xsl:when test="name(.) = 'tocpart'"><xsl:call-template name="href.part" /></xsl:when>
					<!-- DRJ - The below line was previously commented out because it created dead links to non-existent content sections such as pt0001sp0001 (see Squish Content #50) -->
					<!-- This line has been tested successfully and has been uncommented in order to resolve Squish #50 and #144 -->
		      <xsl:when test="name(.) = 'tocsubpart' and ./@role = 'partintro' "><xsl:call-template name="href.part" /></xsl:when>
		      <xsl:otherwise><xsl:call-template name="href.linkinfo" /></xsl:otherwise>
	      </xsl:choose>
	      </xsl:attribute>
	      <xsl:call-template name="toc.title.lookup" /></a>
      </xsl:when>
      <xsl:otherwise>
        <a href="#turnaway" data-toggle="modal">
          <xsl:call-template name="toc.title.lookup" />
        </a>
      </xsl:otherwise>
    </xsl:choose>
  </h3>
  <div class="accordioncontent">
    <ul>
      <xsl:apply-templates select="tocfront|toclevel1|tocchap|tocsubpart|tocback" mode="ritt.toc"></xsl:apply-templates>
    </ul>
  </div>
</xsl:template>

<xsl:template match="toclevel1|toclevel2|toclevel3" mode="ritt.toc">
	<xsl:variable name="depth">
	  <xsl:choose>
	    <xsl:when test="toclevel2[1]/tocentry != '' and $level = 2">1</xsl:when>
	    <xsl:when test="toclevel3[1]/tocentry != '' and $level = 3">1</xsl:when> 
		  <xsl:otherwise>0</xsl:otherwise>
	  </xsl:choose>
	</xsl:variable>

	<xsl:choose>
	  <xsl:when test="$depth = 1"> <!--  or toclevel3[1]/tocentry!=''  or toclevel4[1]/tocentry!=''  -->
	    <li>
        <xsl:choose>
		  <xsl:when test="$disablelinks">
		    <xsl:call-template name="toc.title.lookup" />
		  </xsl:when>
          <xsl:when test="$contentlinks">
		        <a><xsl:attribute name="href"><xsl:call-template name="href.linkinfo" /></xsl:attribute><xsl:call-template name="toc.title.lookup" /></a>
          </xsl:when>
          <xsl:otherwise>
            <a href="#turnaway" data-toggle="modal">
              <xsl:call-template name="toc.title.lookup" />
            </a>
          </xsl:otherwise>
        </xsl:choose>
	    </li>
		  <ul>
		    <xsl:apply-templates select="toclevel1|toclevel2|toclevel3" mode="ritt.toc" />
		  </ul>
	  </xsl:when>
	  <xsl:when test="tocentry = '' and position() = 1"></xsl:when>
	  <!--<xsl:when test="tocentry = '' and position() = last()">
	    <li>
	  	  <a><xsl:attribute name="href"><xsl:call-template name="href.linkinfo" /></xsl:attribute></a>
  	  </li>
	  </xsl:when>-->
	  <xsl:otherwise>
	    <li>
        <xsl:choose>
		  <xsl:when test="$disablelinks"><xsl:call-template name="toc.title.lookup" /></xsl:when>
          <xsl:when test="$contentlinks"><a><xsl:attribute name="href"><xsl:call-template name="href.linkinfo" /></xsl:attribute><xsl:call-template name="toc.title.lookup" /></a></xsl:when>
          <xsl:otherwise>
            <a href="#turnaway" data-toggle="modal">
              <xsl:call-template name="toc.title.lookup" />
            </a>
          </xsl:otherwise>
        </xsl:choose>
	    </li>
    </xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template match="tocfront" mode="ritt.toc" ></xsl:template>

<xsl:template match="tocfront" mode="ritt.toc.front" >
	<xsl:param name="link"><xsl:value-of select="." /></xsl:param>
  <li>
    <xsl:choose>
	  <xsl:when test="$disablelinks">
	  	<xsl:apply-templates select="." />
	  </xsl:when>
      <xsl:when test="$contentlinks">
        <xsl:choose>
          <xsl:when test=". = 'About'">
          	<a><xsl:attribute name="href"><xsl:value-of select="$baseUrl"	/>/resource/title/<xsl:value-of select="$isbndir" /></xsl:attribute><xsl:apply-templates select="." /></a>
          </xsl:when>
          <xsl:when test="$email = 1">
          	<a><xsl:attribute name="href"><xsl:value-of select="$baseUrl"	/>/resource/redirect/<xsl:value-of select="$isbndir" />?section=<xsl:value-of select="@linkend" /></xsl:attribute><xsl:apply-templates select="." /></a>
          </xsl:when>
          <xsl:otherwise>
          	<a><xsl:attribute name="href"><xsl:value-of select="$baseUrl"	/>/resource/detail/<xsl:value-of select="$isbndir" />/<xsl:value-of select="@linkend" /></xsl:attribute><xsl:apply-templates select="." /></a>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <a href="#turnaway" data-toggle="modal">
        	<xsl:apply-templates select="." />
        </a>
      </xsl:otherwise>
    </xsl:choose>
  </li>
</xsl:template>

<!-- Ensure Dedication and About links are always all caps -->
<xsl:template match="text()">
	<xsl:variable name="text" select="translate(., &lowercase;, &uppercase;)"></xsl:variable>
	<xsl:choose>
		<xsl:when test="$text = 'DEDICATION' or $text = 'ABOUT'">
			<xsl:value-of select="$text"></xsl:value-of>
		</xsl:when>
		<xsl:otherwise>
			<xsl:value-of select="." />
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template match="tocback" mode="ritt.toc" >
  <xsl:variable name="toclinkend">
    <xsl:choose>
		  <xsl:when test="@linkend"><xsl:value-of select="@linkend"	/></xsl:when>
		  <xsl:when test="tocentry[1]/@linkend != '' "><xsl:value-of select="tocentry[1]/@linkend" /></xsl:when>
		  <xsl:otherwise>none</xsl:otherwise>
	  </xsl:choose>
  </xsl:variable>

  <xsl:variable name="lablename">
    <xsl:choose>
		  <xsl:when test="substring($toclinkend, 1, 2) = 'ap' ">appendix</xsl:when>
		  <xsl:when test="substring($toclinkend, 1, 2) = 'gl' ">glossary</xsl:when>
		  <xsl:when test="substring($toclinkend, 1, 2) = 'in' ">index</xsl:when>
		  <xsl:when test="substring($toclinkend, 1, 2) = 'bi' ">bibliography</xsl:when>
		  <xsl:when test="tocentry[1]/node() != '' "><xsl:value-of select="tocentry[1]/node()" /></xsl:when>
		  <xsl:otherwise><xsl:value-of select="." /></xsl:otherwise>	
	  </xsl:choose>
  </xsl:variable>	

  <xsl:variable name="lableText">
    <xsl:choose>
		  <xsl:when test="tocentry[1]/node() != '' "><xsl:value-of select="tocentry[1]/node()" /></xsl:when>
		  <xsl:otherwise><xsl:value-of select="." /></xsl:otherwise>	
	  </xsl:choose>
  </xsl:variable>	

  <xsl:if test="$lablename != 'index' ">
    <h3>
      <xsl:choose>
		<xsl:when test="$disablelinks">
			<xsl:value-of select="$lableText" />
		</xsl:when>
        <xsl:when test="$email = 1">
          <a><xsl:attribute name="href"><xsl:value-of select="$baseUrl"	/>/resource/redirect/<xsl:value-of select="$isbndir" />?section=<xsl:value-of select="$toclinkend" /></xsl:attribute><xsl:value-of select="$lableText" /></a>
        </xsl:when>
        <xsl:when test="$contentlinks">
          <a><xsl:attribute name="href"><xsl:value-of select="$baseUrl"	/>/resource/detail/<xsl:value-of select="$isbndir" />/<xsl:value-of select="$toclinkend" /></xsl:attribute><xsl:value-of select="$lableText" /></a>
        </xsl:when>
        <xsl:otherwise>
          <a href="#turnaway" data-toggle="modal">
            <xsl:value-of select="$lableText" />
          </a>
        </xsl:otherwise>
      </xsl:choose>
	  </h3>
  </xsl:if>	
</xsl:template>	

<xsl:template match="*" mode="ritt.back">
  <xsl:value-of select="name()" />	- 
  <xsl:value-of select="." />	
  <xsl:apply-templates select="node()" mode="ritt.back" />
</xsl:template>			

<xsl:template name="toc.title.lookup" >
  <xsl:choose>
	  <xsl:when test="tocentry = ''"><xsl:apply-templates select="../tocentry[1]"	mode="name" /></xsl:when>
	  <xsl:otherwise><xsl:apply-templates select="tocentry"	mode="name" /></xsl:otherwise>
  </xsl:choose>
</xsl:template>
	
<xsl:template match="text" mode="name"><xsl:value-of select="." /></xsl:template>	
<xsl:template match="sub" mode="name"><sub><xsl:apply-templates mode="name" /></sub></xsl:template>	
<xsl:template match="sup" mode="name"><sup><xsl:apply-templates mode="name" /></sup></xsl:template>	
<xsl:template match="*" mode="name"><xsl:apply-templates mode="name" /></xsl:template>	
	
<xsl:template name="href.part" >
	<xsl:variable name="linkend" >
		<xsl:choose>
			<xsl:when test="./@role = 'partintro'">
				<xsl:value-of select="tocentry/@linkend" />
			</xsl:when>
			<xsl:when test="tocback[1][not(preceding-sibling::toclevel1)]/@linkend">
				<xsl:value-of select="tocback[1]/@linkend"	/>
			</xsl:when>
			<xsl:when test="tocback[1][not(preceding-sibling::toclevel1)]/tocentry[1]/@linkend != '' ">
				<xsl:value-of select="tocback[1]/tocentry[1]/@linkend" />
			</xsl:when>
		</xsl:choose>
	</xsl:variable>
	<xsl:choose>
		<xsl:when test="$linkend != ''">
			<xsl:value-of select="$baseUrl"	/>/resource/detail/<xsl:value-of select="$isbndir" />/<xsl:value-of select="$linkend" />
		</xsl:when>
		<xsl:otherwise>
			<xsl:call-template name="href.linkinfo" />
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="href.linkinfo" >
<xsl:variable name="sectParent" >
	<xsl:choose>
		<xsl:when test="name(.) = 'toclevel1' "><xsl:value-of select="tocentry/@linkend" /></xsl:when> 
		<xsl:when test="name(ancestor::toclevel1)='toclevel1'"><xsl:value-of select="(ancestor::toclevel1)/tocentry/@linkend" /></xsl:when> 
		<xsl:when test="name(descendant::toclevel1)='toclevel1'"><xsl:value-of select="(descendant::toclevel1)/tocentry/@linkend" /></xsl:when> 
	</xsl:choose>	
</xsl:variable>
<xsl:variable name="ChapterParent" >
	<xsl:choose>
		<xsl:when test="name(.) = 'tocchap' "><xsl:value-of select="tocentry/@linkend" /></xsl:when> 
		<xsl:when test="name(ancestor::tocchap)='tocchap'"><xsl:value-of select="(ancestor::tocchap)/tocentry/@linkend" /></xsl:when> 
	</xsl:choose>	
</xsl:variable>

	<xsl:choose>
    <xsl:when test="$sectParent != ''">
      <xsl:choose>
        <xsl:when test="$email = 1">
          <xsl:value-of select="$baseUrl"	/>/resource/redirect/<xsl:value-of select="$isbndir" />?section=<xsl:value-of select="$sectParent" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$baseUrl"	/>/resource/detail/<xsl:value-of select="$isbndir" />/<xsl:value-of select="$sectParent" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:otherwise>#<xsl:value-of select="tocentry/@linkend" /></xsl:otherwise>
	</xsl:choose>		
</xsl:template>

<xsl:template match="tocinfo"	mode="ritt.toc"></xsl:template>	
<xsl:template match="*" mode="ritt.toc"></xsl:template>		
</xsl:stylesheet>