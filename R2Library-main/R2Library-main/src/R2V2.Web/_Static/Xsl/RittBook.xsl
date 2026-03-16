<?xml version='1.0'?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                exclude-result-prefixes="xsl html"
                xmlns:html="http://www.w3.org/TR/xhtml1/transitional"                
                version='1.0'>
  
<xsl:import href="html/docbook.xsl"/>
<xsl:preserve-space elements="*"/>
<xsl:include href="table.ritt.xsl"/>
<xsl:include href="footnote.ritt.xsl"/>
<xsl:include href="bibliomixed.ritt.xsl " />	
<xsl:include href="subpart.ritt.xsl " />	
<xsl:include href="common.ritt.xsl" />
<xsl:include href="ulink.ritt.xsl " />	
<xsl:include href="link.ritt.xsl " />	
<xsl:include href="titlepage.ritt.xsl" />
<xsl:include href="sections.ritt.xsl" />
<xsl:include href="titlepage.templates.ritt.xsl" />
<xsl:include href="para.ritt.xsl" />
<xsl:include href="admon.ritt.xsl" />

<xsl:param name="objectid"></xsl:param>	
<xsl:param name="version">1.1</xsl:param>	
<xsl:param name="isbndir"><xsl:choose><xsl:when test="//risinfo[1]/isbn"><xsl:value-of select="//risinfo[1]/isbn" /></xsl:when>
<xsl:when test="//bookinfo[1]/isbn"><xsl:value-of select="//bookinfo[1]/isbn" /></xsl:when></xsl:choose></xsl:param>	
<xsl:param name="css.decoration" select="1"/>
<xsl:param name="generate.toc"></xsl:param>	 
<xsl:param name="baseUrl" > </xsl:param>
<xsl:param name="imageBaseUrl" > </xsl:param>
<xsl:param name="bibliography.numbered">1</xsl:param>
<xsl:param name="email" select="0"	/>
<xsl:param name="runinhead.title.end.punct" select="'.!?:'"/>
  
<xsl:template match="/">
  <div>
    <xsl:if test=" $rootid != '' ">
	      <xsl:if test="count(key('id',$rootid)) = 0">
	        <xsl:message terminate="yes">
		        <xsl:text>ID '</xsl:text>
		        <xsl:value-of select="$rootid"/>
		        <xsl:text>' not found in document.</xsl:text>
	        </xsl:message>
	      </xsl:if>
    </xsl:if>	
  
    <xsl:choose>
	    <xsl:when test="$rootid != ''">
		    <xsl:choose>
		      <xsl:when test="substring($rootid,1,2) = 'pt' ">
    	      <xsl:apply-templates select="key('id',$rootid)/partintro" />
	        </xsl:when>	    
	        <xsl:otherwise>
		        <xsl:apply-templates select="key('id',$rootid)" mode="process.root"/>
		        <xsl:if test="$tex.math.in.alt != ''">
		          <xsl:apply-templates select="//*[@id=$rootid]" mode="collect.tex.math"/>
		        </xsl:if>
	        </xsl:otherwise>
	      </xsl:choose>
	    </xsl:when>
	    <xsl:otherwise>
	      <xsl:apply-templates select="/" mode="process.root"/>
	      <xsl:if test="$tex.math.in.alt != ''">
	        <xsl:apply-templates select="/" mode="collect.tex.math"/>
	      </xsl:if>
	    </xsl:otherwise>
    </xsl:choose>

	  <xsl:choose>
		  <xsl:when test="local-name(/*) = 'preface' or local-name(/*) = 'appendix'">
			</xsl:when>
		  <xsl:when test="$rootid != ''">
				<!--<xsl:for-each select="key('id',$rootid)//footnote">-->
					<xsl:choose>
						<xsl:when test="substring($rootid,1,2) = 'pt' ">
							<xsl:call-template name="process.footnotes">
								<xsl:with-param name="footnotes" select="key('id',$rootid)/partintro//footnote" />
							</xsl:call-template>
						</xsl:when>	    
						<xsl:otherwise>
							<xsl:call-template name="process.footnotes">
								<xsl:with-param name="footnotes" select="key('id',$rootid)//footnote" />
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>
				<!--</xsl:for-each>-->
			</xsl:when>
		  <xsl:otherwise>
				<!--<xsl:for-each select=".//footnote">-->
					<xsl:call-template name="process.footnotes">
						<xsl:with-param name="footnotes" select=".//footnote" />
					</xsl:call-template>
				<!--</xsl:for-each>-->
			</xsl:otherwise>	
	  </xsl:choose>
  </div>
</xsl:template>

<!-- Excess whitespace removal -->
<xsl:template match="text()" mode="bibliomixed.mode">
	<xsl:apply-templates select="." />
</xsl:template>

<xsl:template match="text()">
	<xsl:choose>
		<xsl:when test="normalize-space(.) = ''">
			<xsl:choose>
				<xsl:when test="following-sibling::*[1][self::superscript]|following-sibling::*[1][self::subscript]|parent::superscript|parent::subscript">
					<!-- Ignore whitespace preceding subscripts/superscripts -->
					<!-- Ignore leading/trailing whitespace within subscripts/superscripts -->
				</xsl:when>
				<xsl:when test="following-sibling::*[1][self::link and child::*[1][self::superscript|self::subscript]]">
					<!-- Ignore whitespace preceding links having subscript/superscript as first child -->
				</xsl:when>				
				<xsl:when test="parent::emphasis and (preceding-sibling::*[1][self::emphasis]|following-sibling::*[1][self::emphasis])">
					<!-- Ignore leading/trailing whitespace within double-nested emphasis -->
				</xsl:when>
				<xsl:when test="parent::emphasis and (preceding-sibling::*[1][self::ulink]|following-sibling::*[1][self::ulink])">
					<!-- Ignore leading/trailing whitespace for link within emphasis -->
				</xsl:when>
				<xsl:when test="parent::ulink and (preceding-sibling::*[1][self::emphasis]|following-sibling::*[1][self::emphasis])">
					<!-- Ignore leading/trailing whitespace for emphasis within link -->
				</xsl:when>
				<xsl:when test="parent::email and (preceding-sibling::*[1][self::emphasis]|following-sibling::*[1][self::emphasis])">
					<!-- Ignore leading/trailing whitespace for emphasis within email -->
				</xsl:when>
				<xsl:when test="parent::title and (preceding-sibling::*[1][self::emphasis]|following-sibling::*[1][self::emphasis])">
					<!-- Ignore leading/trailing whitespace for emphasis within title -->
				</xsl:when>
				<xsl:when test="ancestor::bibliomixed and parent::address and (not(preceding-sibling::*) or not(following-sibling::*))">
					<!-- Ignore leading/trailing whitespace for address descendant from bibliomixed -->
				</xsl:when>
        <xsl:when test="parent::link and not(following-sibling::*[1][self::ulink])">
          <!-- Ignore leading/trailing whitespace within links when not before ulink -->
        </xsl:when>
        <xsl:when test="parent::bibliomixed and preceding-sibling::*[1][self::title] and following-sibling::*[1][self::title]
													and substring(preceding-sibling::*[1][self::title], (string-length(preceding-sibling::*[1][self::title]))) != ','">
          <!-- Ignore leading/trailing whitespace between titles within bibliomixed if preceding title does not end with a comma -->
        </xsl:when>
        <xsl:when test="parent::bibliomixed and preceding-sibling::*[1][self::title] and following-sibling::*[1][self::volumenum]">
          <!-- Ignore leading/trailing whitespace between title and volumenum within bibliomixed -->
        </xsl:when>
        <xsl:when test="parent::bibliomixed and preceding-sibling::*[1][self::title] and following-sibling::*[1][self::address]">
          <!-- Ignore leading/trailing whitespace between title and address within bibliomixed -->
        </xsl:when>
        <xsl:when test="parent::volumenum and following-sibling::*[1][self::emphasis]">
          <!-- Ignore leading whitespace before emphasis within volumenum -->
        </xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="."/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:when>
		<xsl:otherwise>
			<xsl:value-of select="."/>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template match="footnote" mode="process.filtered.footnote">
  <xsl:choose>
	  <xsl:when test="ancestor::table" />
	  <xsl:when test="ancestor::book" />
	  <xsl:when test="local-name(..) = 'title' and not(ancestor::appendix or ancestor::preface)"><xsl:apply-templates mode="process.footnote.mode" select="."	/></xsl:when>
	  <xsl:when test="local-name(..) = 'chaptertitle' and ancestor::sect1/title != '' " />
	  <xsl:when test="local-name(..) = 'chaptersubtitle' and ancestor::sect1/title != '' " />
	  <xsl:when test="ancestor::glossary" />
	  <xsl:when test="ancestor::sect1[not(ancestor::appendix or ancestor::preface)]"><xsl:apply-templates mode="process.footnote.mode" select="."	/></xsl:when> 
  </xsl:choose>
</xsl:template>

<!-- Add other variable definitions here -->
<xsl:template match="biblioid" mode="bibliography.mode">
	 <xsl:choose>
			<xsl:when test="@otherclass='PubMedID'">
			  <a target="_blank"><xsl:attribute name="href">https://www.ncbi.nlm.nih.gov/entrez/query.fcgi?cmd=Retrieve&amp;db=pubmed&amp;dopt=Abstract&amp;list_uids=<xsl:apply-templates mode="bibliography.mode"/></xsl:attribute>	 [PMID <xsl:apply-templates mode="bibliography.mode"/>]</a>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates mode="bibliography.mode"/>
			</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template match="abstract" mode="bibliography.mode"><xsl:apply-templates  mode="bibliography.mode" /></xsl:template>

<xsl:template match="bibliography" mode="title.markup">
  <xsl:param name="allow-anchors" select="0"/>
  <xsl:variable name="title" select="(bibliographyinfo/title|info/title|title)[1]"/>
  <xsl:choose>
    <xsl:when test="$title">
      <xsl:apply-templates select="$title" mode="title.markup">
        <xsl:with-param name="allow-anchors" select="$allow-anchors"/>
      </xsl:apply-templates>
    </xsl:when>
  </xsl:choose>
</xsl:template>
  
<xsl:template name="partintro.titlepage">
  <xsl:choose>
	  <xsl:when test="title != '' "></xsl:when>
	  <xsl:otherwise>
			  <xsl:apply-templates mode="titlepage.mode" select="../title" />
	  </xsl:otherwise>	
  </xsl:choose>
  <xsl:call-template name="partintro.titlepage.before.recto"/>
  <xsl:call-template name="partintro.titlepage.recto"/>
  <xsl:call-template name="partintro.titlepage.before.verso"/>
  <xsl:call-template name="partintro.titlepage.verso"/>
</xsl:template>
  
<xsl:template match="title" mode="titlepage.mode">
  <xsl:variable name="id">

  <xsl:choose>
    <xsl:when test="contains(local-name(..), 'info')">
      <xsl:call-template name="object.id">
        <xsl:with-param name="object" select="../.."/>
      </xsl:call-template>
    </xsl:when>
    <xsl:otherwise>
      <xsl:call-template name="object.id">
        <xsl:with-param name="object" select=".."/>
      </xsl:call-template>
    </xsl:otherwise>
  </xsl:choose>
</xsl:variable>
<xsl:variable name="value"><xsl:value-of select="."/></xsl:variable>
  <xsl:choose>
    <xsl:when test="$value = '' and ../bibliography/@id and contains(translate(ancestor::sect1/title,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'bibliography')"></xsl:when>
    <xsl:when test="$value = '' and ../bibliography[preceding-sibling::para or preceding-sibling::formalpara]/@id "></xsl:when>
    <xsl:when test="$value = '' and ../bibliography/@id ">Bibliography</xsl:when>
    <xsl:when test="$value = '' and name(..) = 'sect1'">
      <h2 id="{$id}">
        <xsl:apply-templates select="../sect1info/risinfo/chaptertitle" />
      </h2>
			<h3 class="subtitle">
				<xsl:if test="preceding-sibling::sect1info/risinfo/chaptersubtitle"><xsl:apply-templates select="../sect1info/risinfo/chaptersubtitle" /></xsl:if>
			</h3>
    </xsl:when>
    <xsl:when test="$value = '' and name(..) = 'partintro' ">
      <xsl:value-of select="../../title"	/>
    </xsl:when>
    <xsl:when test="$show.revisionflag != 0 and @revisionflag">
      <h2 id="{$id}">
        <xsl:apply-templates mode="titlepage.mode"/>
      </h2>
    </xsl:when>
    <xsl:when test="name(..) = 'sect1'">
      <h2 id="{$id}">
        <xsl:apply-templates mode="titlepage.mode"/>
        <xsl:apply-templates select="../partinfo/authorgroup"  mode="titlepage.mode"/>
      </h2>
    </xsl:when>
    <xsl:when test="name(..) = 'sect2'">
      <h3 id="{$id}">
        <xsl:apply-templates mode="titlepage.mode"/>
        <xsl:apply-templates select="../partinfo/authorgroup"  mode="titlepage.mode"/>
      </h3>
    </xsl:when>
    <xsl:when test="name(..) = 'sect3'">
      <h4 id="{$id}">
        <xsl:apply-templates mode="titlepage.mode"/>
        <xsl:apply-templates select="../partinfo/authorgroup"  mode="titlepage.mode"/>
      </h4>
    </xsl:when>
  	<xsl:when test="name(..) = 'sect4'">
  		<h5 id="{$id}">
  			<xsl:apply-templates mode="titlepage.mode"/>
  			<xsl:apply-templates select="../partinfo/authorgroup"  mode="titlepage.mode"/>
  		</h5>
  	</xsl:when>
  	<xsl:when test="name(..) = 'sect5'">
  		<h6 id="{$id}">
  			<xsl:apply-templates mode="titlepage.mode"/>
  			<xsl:apply-templates select="../partinfo/authorgroup"  mode="titlepage.mode"/>
  		</h6>
  	</xsl:when>
  	<xsl:when test="name(..) = 'sect6'">
  		<h6 id="{$id}">
  			<xsl:apply-templates mode="titlepage.mode"/>
  			<xsl:apply-templates select="../partinfo/authorgroup"  mode="titlepage.mode"/>
  		</h6>
  	</xsl:when>
	  <xsl:when test="name(..) = 'sect7' or name(..) = 'sect8' or name(..) = 'sect9' or name(..) = 'sect10'">
  		<h6 id="{$id}" class="{name(..)}">
  			<xsl:apply-templates mode="titlepage.mode"/>
  			<xsl:apply-templates select="../partinfo/authorgroup"  mode="titlepage.mode"/>
  		</h6>
  	</xsl:when>		 	
    <xsl:otherwise>
      <h2 id="{$id}">
        <xsl:apply-templates mode="titlepage.mode"/>
      </h2>
      <xsl:apply-templates select="../partinfo/authorgroup" mode="titlepage.mode"/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template match="table|figure|equation" mode="delayed">
  <xsl:param name="inline" select="0" />
  <xsl:choose>
    <xsl:when test="$inline = 1"><xsl:call-template name="calsTable" /></xsl:when>	
    <!-- suppress things that should be printed by the inline -->
    <xsl:when test="ancestor::note and $inline = 0"></xsl:when>
    <xsl:when test="ancestor::sidebar and $inline = 0"></xsl:when>
	  <xsl:when test="ancestor::important and $inline = 0"></xsl:when>
	  <xsl:when test="ancestor::warning and $inline = 0"></xsl:when>
	  <xsl:when test="ancestor::caution and $inline = 0"></xsl:when>
	  <xsl:when test="ancestor::tip and $inline = 0 "></xsl:when>	
	  <xsl:when test="ancestor::blockquote and $inline = 0 "></xsl:when>	
    <xsl:when test="graphic">
      <div class="figure clearfix">
        <xsl:attribute name="data-user-content-image">imageSource=/images/<xsl:value-of select="$isbndir" />/<xsl:value-of select="graphic/@fileref" /></xsl:attribute>
        <xsl:attribute name="id"><xsl:value-of select="./@id" /></xsl:attribute>
        <div class="figimage">
          <a data-view="inline-resource" href="#enlarge-image">
		        <xsl:element name="img">
              <xsl:attribute name="src">
                <xsl:value-of select="$imageBaseUrl" />/<xsl:value-of select="$isbndir" />/<xsl:value-of select="graphic/@fileref" />
              </xsl:attribute>
            </xsl:element>
          </a>
        </div>  
        <div class="figcaption">
			    <xsl:call-template name="formal.object.heading"/>
          <ul class="actions-figure clearfix">
            <li><a class="ir btn-image-enlarge" data-view="inline-resource" href="#enlarge-image">Enlarge Image</a></li>
          </ul>
        </div>
      </div>
    </xsl:when>
    <xsl:when test="mediaobject/imageobject">
      <div class="figure clearfix">
        <!-- Ideal implementation:
        <xsl:attribute name="data-user-content-image">{&quot;fileName&quot;:&quot;<xsl:value-of select="mediaobject/imageobject/imagedata/@fileref" />&quot;, &quot;sectionTitle&quot;:&quot;<xsl:value-of select="ancestor::sect1/title"/>&quot;, &quot;sectionId&quot;:&quot;<xsl:value-of select="ancestor::sect1/@id"/>&quot;, &quot;title&quot;:&quot;<xsl:value-of select="title" />&quot;}</xsl:attribute>
        -->
				  <xsl:variable name="sectionId">
						<xsl:choose>
							<xsl:when test="ancestor::appendix">
								<xsl:value-of select="ancestor::appendix/@id" />
							</xsl:when>
							<xsl:when test="ancestor::preface">
								<xsl:value-of select="ancestor::preface/@id" />
							</xsl:when>
							<xsl:when test="ancestor::subpart">
								<xsl:value-of select="ancestor::subpart/@id" />
							</xsl:when>
							<xsl:when test="ancestor::part">
								<xsl:value-of select="ancestor::part/@id" />
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="ancestor::sect1/@id" />
							</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>
        <xsl:attribute name="data-user-content-image">imageSource=/images/<xsl:value-of select="$isbndir" />/<xsl:value-of select="mediaobject/imageobject/imagedata/@fileref" />&amp;fileName=<xsl:value-of select="mediaobject/imageobject/imagedata/@fileref" />&amp;sectionTitle=<xsl:value-of select="ancestor::sect1/title"/>&amp;sectionId=<xsl:value-of select="$sectionId"/>&amp;title=<xsl:value-of select="substring(title, 1, 300)" /></xsl:attribute><!-- this will be trimmed by the backend code to 255 at a word boundary and have an ellipsis appended to it -->
        <xsl:attribute name="id"><xsl:value-of select="./@id" /></xsl:attribute>
        <div class="figimage">
          <a data-target=".modal.enlarge" href="#enlarge-image">
            <xsl:element name="img">
              <xsl:attribute name="src">
                <xsl:value-of select="$imageBaseUrl" />/<xsl:value-of select="$isbndir" />/<xsl:value-of select="mediaobject/imageobject/imagedata/@fileref" />
              </xsl:attribute>
            </xsl:element>
          </a>
        </div>
        <div class="figcaption">
          <xsl:call-template name="formal.object.heading"/>
          <xsl:apply-templates select="mediaobject/caption" />
          <xsl:if test="$email = 0">
            <ul class="actions-figure clearfix">
              <li><a class="ir btn-image-save" data-toggle="modal" data-target=".modal.save" href="#save-image">Save Image</a></li>
              <li><a class="ir btn-image-enlarge" data-target=".modal.enlarge" href="#enlarge-image">Enlarge Image</a></li>
            </ul>
          </xsl:if>
        </div>
      </div>
    </xsl:when>
    <xsl:when test="tgroup">
      <div class="figure clearfix">
        <xsl:attribute name="id"><xsl:value-of select="./@id" /></xsl:attribute>
				<div class="topscrollwrapper"><div class="topscroll"></div></div>
        <div class="figtable">
          <xsl:call-template name="notitle.formal.object" />
        </div>
        <div class="figcaption">
          <xsl:call-template name="formal.object.heading"/>
          <xsl:if test="$email = 0">
            <ul class="actions-figure clearfix">
              <li><a data-view="inline-resource"><xsl:attribute name="class">ir btn-<xsl:value-of select="name(.)" />-view</xsl:attribute>ir" />View <xsl:value-of select="name(.)" /></a></li>
            </ul>
          </xsl:if>
        </div>
	    </div>
    </xsl:when>
    <xsl:otherwise>
      <div class="figure clearfix">
        <xsl:attribute name="id"><xsl:value-of select="./@id" /></xsl:attribute>
        <div class="figtable">
          <xsl:copy>
            <xsl:copy-of select="@*"/>
            <xsl:call-template name="htmlTable"/>
          </xsl:copy>
        </div>
        <div class="figcaption">
          <xsl:call-template name="formal.object.heading"/>
        </div>
      </div>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template match="table">
<xsl:choose>
	<xsl:when test="ancestor::note"><xsl:apply-templates mode="delayed" select="."	><xsl:with-param name="inline" select="1"	/></xsl:apply-templates></xsl:when>
	<xsl:when test="ancestor::sidebar"><xsl:apply-templates mode="delayed" select="."><xsl:with-param name="inline" select="1" /></xsl:apply-templates></xsl:when>
	<xsl:when test="ancestor::important"><xsl:apply-templates mode="delayed" select="."	><xsl:with-param name="inline" select="1"	/></xsl:apply-templates></xsl:when>
	<xsl:when test="ancestor::warning"><xsl:apply-templates mode="delayed" select="."	><xsl:with-param name="inline" select="1"	/></xsl:apply-templates></xsl:when>
	<xsl:when test="ancestor::caution"><xsl:apply-templates mode="delayed" select="."	><xsl:with-param name="inline" select="1"	/></xsl:apply-templates></xsl:when>
	<xsl:when test="ancestor::tip"><xsl:apply-templates mode="delayed" select="."	><xsl:with-param name="inline" select="1"	/></xsl:apply-templates></xsl:when>
	<xsl:when test="ancestor::blockquote"><xsl:apply-templates mode="delayed" select="."	><xsl:with-param name="inline" select="1"	/></xsl:apply-templates></xsl:when>
	<xsl:when test="name(..) != 'para' "><xsl:apply-templates mode="delayed" select="."	/>
</xsl:when>
</xsl:choose>
</xsl:template>

<xsl:template match="figure|equation">
<xsl:choose>
	<xsl:when test="name(..) = 'para' "></xsl:when>
	<xsl:when test="ancestor::note"><xsl:apply-templates mode="delayed" select="." ><xsl:with-param name="inline" select="2"	/></xsl:apply-templates></xsl:when>
	<xsl:when test="ancestor::sidebar"><xsl:apply-templates mode="delayed" select="." ><xsl:with-param name="inline" select="2"	/></xsl:apply-templates></xsl:when>
	<xsl:when test="ancestor::important"><xsl:apply-templates mode="delayed" select="." ><xsl:with-param name="inline" select="2"	/></xsl:apply-templates></xsl:when>
	<xsl:when test="ancestor::warning"><xsl:apply-templates mode="delayed" select="." ><xsl:with-param name="inline" select="2"	/></xsl:apply-templates></xsl:when>
	<xsl:when test="ancestor::caution"><xsl:apply-templates mode="delayed" select="." ><xsl:with-param name="inline" select="2"	/></xsl:apply-templates></xsl:when>
	<xsl:when test="ancestor::tip"><xsl:apply-templates mode="delayed" select="." ><xsl:with-param name="inline" select="2"	/></xsl:apply-templates></xsl:when>
	<xsl:when test="name(..) != 'para' "><xsl:apply-templates mode="delayed" select="."	><xsl:with-param name="inline" select="2"	/></xsl:apply-templates></xsl:when>
</xsl:choose>
</xsl:template>

<xsl:template match="title|caption" mode="htmlTable" ></xsl:template>		

<xsl:template name="notitle.formal.object" >
  <!--<xsl:call-template name="anchor">
    <xsl:with-param name="conditional" select="0"/>
  </xsl:call-template>-->

  <!-- HACK: This doesn't belong inside formal.object; it should be done by -->
  <!-- the table template, but I want the link to be inside the DIV, so... -->
  <xsl:if test="local-name(.) = 'table'">
    <xsl:call-template name="table.longdesc"/>
  </xsl:if>

  <xsl:for-each select="tgroup">
    <xsl:call-template name="tgroup2" />
  </xsl:for-each>
</xsl:template>

<xsl:template name="formal.object.heading">
	<xsl:choose>
		<xsl:when test="self::table and ancestor::note
																	 |ancestor::sidebar
																	 |ancestor::important
																	 |ancestor::warning
																	 |ancestor::caution
																	 |ancestor::tip
																	 |ancestor::blockquote">
		</xsl:when>
		<xsl:otherwise>
			<xsl:variable name="inVideoSection">
				<xsl:call-template name="in.video.section" />
			</xsl:variable>
			<xsl:comment> field: r2ImageTitle </xsl:comment>
			<p>
				<xsl:call-template name="substitute-markup">
					<xsl:with-param name="allow-anchors" select="1"/>
					<xsl:with-param name="template" >%t</xsl:with-param>
				</xsl:call-template>
			</p>
			<xsl:choose>
				<xsl:when test="$inVideoSection = 1">
					<xsl:comment> field: r2VideoSection</xsl:comment>
					<xsl:message>{ "messageType": "video", "isbn": "<xsl:value-of select="$isbndir" />", "section": "<xsl:value-of select="/*/@id" />", "mediaUrl": "<xsl:value-of select="//videodata/@fileref" />"}</xsl:message>
				</xsl:when>
				<xsl:otherwise><xsl:comment> field: </xsl:comment></xsl:otherwise>
			</xsl:choose>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template match="hit"><span class='hit'><xsl:attribute name="id">hit<xsl:value-of select="@number" /></xsl:attribute><xsl:value-of select="."	/></span></xsl:template>		

<xsl:template match="risinfo"></xsl:template>

<xsl:template match="r2ChapterSearch"></xsl:template>

<xsl:template match="dedication">
    <xsl:call-template name="language.attribute"/>
    <xsl:call-template name="dedication.titlepage"/>
    <xsl:apply-templates/>
    <xsl:call-template name="process.footnotes"/>
</xsl:template>

<xsl:template match="inlineequation">
  <xsl:element name="img">
    <xsl:attribute name="src"><xsl:value-of select="$imageBaseUrl"	/>/<xsl:value-of select="$isbndir" />/<xsl:value-of select="graphic/@fileref" /></xsl:attribute>
  </xsl:element>
</xsl:template>
  
<xsl:template match="inlinemediaobject">
    <xsl:if test="imageobject/imagedata/@fileref != 'pm.jpg'">
      <xsl:element name="img">
		<xsl:attribute name="class">inline-media</xsl:attribute>
        <xsl:attribute name="src"><xsl:value-of select="$imageBaseUrl"	/>/<xsl:value-of select="$isbndir" />/<xsl:value-of select="imageobject/imagedata/@fileref" /></xsl:attribute>
      </xsl:element>
    </xsl:if>
</xsl:template>

<xsl:template match="processing-instruction('lbl')">
  <br />
</xsl:template>

<xsl:template match="processing-instruction('lb')">
  <br />
</xsl:template>

<xsl:template match="mediaobject">
<xsl:choose>
	<xsl:when test="imageobject">
    <xsl:element name="img">
      <xsl:attribute name="src"><xsl:value-of select="$imageBaseUrl"	/>/<xsl:value-of select="$isbndir" />/<xsl:value-of select="imageobject/imagedata/@fileref" /></xsl:attribute>
    </xsl:element>
  </xsl:when>
	<xsl:when test="videoobject">
		<xsl:apply-templates/>
	</xsl:when>
	<xsl:when test="audioobject">
		<xsl:apply-templates/>
	</xsl:when>
</xsl:choose>	
</xsl:template>
<xsl:template match="chaptertitle"><xsl:apply-templates /></xsl:template>			
<xsl:template match="chaptersubtitle"><xsl:apply-templates /></xsl:template>

</xsl:stylesheet>