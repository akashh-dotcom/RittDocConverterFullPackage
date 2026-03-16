<?xml version='1.0' encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:exsl="http://exslt.org/common"
                exclude-result-prefixes="exsl"
                version='1.0'>


<xsl:key name="id" match="*" use="@linkend" />
<xsl:param name="objectid"></xsl:param>	
<xsl:variable name="workingNode" select="key('id',$objectid)" />
<xsl:variable name="ancestorType">
	<xsl:choose>
		<xsl:when test="local-name($workingNode) = 'tocentry'">
			<xsl:apply-templates mode="ancestor.type" select="$workingNode"/> 
		</xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="ancestor.type" select="$workingNode"/></xsl:otherwise>	
	</xsl:choose>	
</xsl:variable>
<xsl:variable name="locobjectid">
	<xsl:choose>
		<xsl:when test="local-name($workingNode) = 'tocentry' "><xsl:apply-templates mode="linkparent" select="$workingNode"/></xsl:when>
		<xsl:otherwise><xsl:value-of select="$workingNode/@linkend"	/></xsl:otherwise>	
	</xsl:choose>	
</xsl:variable>	

<xsl:output method="xml" encoding="UTF-8" indent="no" /> 

<xsl:template match="/"> 
<!-- <xsl:message><xsl:value-of select="$locobjectid" /> type is <xsl:value-of select="local-name(//*[@linkend=$locobjectid])"/> 
<xsl:if test="local-name(//*[@linkend=$locobjectid]) = 'tocentry' ">
parent type is <xsl:value-of select="local-name(//*[tocentry/@linkend=$locobjectid])"/></xsl:if>
ancestor type is <xsl:value-of select="$ancestorType"/>
</xsl:message>
-->
<navset>
	<booktitle><xsl:value-of select="toc/title"	/></booktitle>
	<!--<xsl:message>
		name of ancestor type="<xsl:value-of select="$ancestorType"	/>" 
		name of parent node="<xsl:value-of select="local-name($workingNode/..) "	/>" 
		name of node="<xsl:value-of select="local-name($workingNode) "	/>" 
		node to match="<xsl:value-of select="$locobjectid"	/>"
	</xsl:message>-->	
	<xsl:if test="$locobjectid != '' or $ancestorType = 'tocfront' " >
		<xsl:choose>
			<xsl:when test="$ancestorType = 'tocfront' "><xsl:apply-templates mode="ritt.nav" select="$workingNode" /></xsl:when>
			<xsl:when test="$ancestorType = 'tocchap' ">
				<xsl:if test="local-name($workingNode/..) = 'toclevel1' ">
				<xsl:apply-templates mode="ritt.nav" select="$workingNode/.." />
				</xsl:if>	
				<!-- ## 25/11/05 For sect2 link Test -->
				<xsl:if test="local-name($workingNode/..) = 'toclevel2'"><xsl:apply-templates mode="ritt.nav" select="$workingNode/../.." /></xsl:if>
				<xsl:if test="local-name($workingNode/..) = 'toclevel3'"><xsl:apply-templates mode="ritt.nav" select="$workingNode/../../.." /></xsl:if>
				<xsl:if test="local-name($workingNode/..) = 'toclevel4'"><xsl:apply-templates mode="ritt.nav" select="$workingNode/../../../.." /></xsl:if>
				<xsl:if test="local-name($workingNode/..) = 'toclevel5'"><xsl:apply-templates mode="ritt.nav" select="$workingNode/../../../../.." /></xsl:if>
				<!-- END -->
				<xsl:if test="local-name($workingNode/..) = 'tocchap' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../toclevel1[1]" /></xsl:if>	
				
				<!-- ## 26/11/05 For sect2 link Test -->
				<xsl:if test="local-name($workingNode/..) = 'tocchap' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../../toclevel2[1]" /></xsl:if>	
				<!-- <xsl:if test="local-name($workingNode/..) = 'tocchap' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../../../toclevel3[1]" /></xsl:if>	
				<xsl:if test="local-name($workingNode/..) = 'tocchap' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../../../../toclevel4[1]" /></xsl:if>	
				<xsl:if test="local-name($workingNode/..) = 'tocchap' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../../../../../toclevel5[1]" /></xsl:if>	
				END -->
			</xsl:when>
			<xsl:when test="$ancestorType = 'chapter-part' ">
				<xsl:if test="local-name($workingNode/..) = 'toclevel1' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/.." />
				</xsl:if>

				<!-- ## 27/12/05 & 28/12/05 For sect2 link Test -->
				<xsl:if test="local-name($workingNode/..) = 'toclevel2' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../.." />
				</xsl:if> 
				<xsl:if test="local-name($workingNode/..) = 'toclevel3' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../../.." />
				</xsl:if> 
				<xsl:if test="local-name($workingNode/..) = 'toclevel4' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../../../.." />
				</xsl:if> 
				<xsl:if test="local-name($workingNode/..) = 'toclevel5' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../../../../.." />
				</xsl:if> 
	
				<xsl:if test="local-name($workingNode/..) = 'tocchap' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../toclevel1[1]" />
				</xsl:if>
				<!-- ## 27/12/05 & 28/12/05 For sect2 link Test -->
				<xsl:if test="local-name($workingNode/..) = 'tocchap' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../../toclevel2[1]" />
				</xsl:if>	
				<xsl:if test="local-name($workingNode/..) = 'tocchap' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../../../toclevel3[1]" />
				</xsl:if>	
			</xsl:when>
			<xsl:when test="$ancestorType = 'tocback' ">
				<xsl:if test="local-name($workingNode) = 'tocback' "><xsl:apply-templates mode="ritt.nav" select="$workingNode" />
				</xsl:if>	
				<xsl:if test="local-name($workingNode/..) = 'tocback' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/.." />
				</xsl:if>	
			</xsl:when>
			<xsl:when test="$ancestorType = 'tocpart' ">
				<xsl:if test="local-name($workingNode/..) = 'toclevel1' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/.." />
				</xsl:if>	
				<xsl:if test="local-name($workingNode/..) = 'tocchap' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../toclevel1[1]" />
				</xsl:if>
				<!-- **05-12-06** updated for working notde should not be tocback -->
				<xsl:if test="local-name($workingNode/..) = 'tocpart' and local-name($workingNode) != 'tocback'"><xsl:apply-templates mode="ritt.nav" select="$workingNode/.." />
				</xsl:if>	
				<xsl:if test="local-name($workingNode/..) = 'tocsubpart' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/.." />
				</xsl:if>
<!-- **04-12-06** For appendix appearing inside part (2 matches)	 -->
				<xsl:if test="local-name($workingNode) = 'tocback' ">
					<xsl:apply-templates mode="ritt.nav" select="$workingNode" />
				</xsl:if>	
				<xsl:if test="local-name($workingNode/..) = 'tocback' ">
					<xsl:apply-templates mode="ritt.nav" select="$workingNode/.." />
				</xsl:if>
			</xsl:when>
			<xsl:when test="$ancestorType = 'tocsubpart' ">
				<xsl:if test="local-name($workingNode/..) = 'toclevel1' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/.." />
				</xsl:if>	
				<xsl:if test="local-name($workingNode/..) = 'tocchap' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/../toclevel1[1]" />
				</xsl:if>	
				<xsl:if test="local-name($workingNode/..) = 'tocsubpart' "><xsl:apply-templates mode="ritt.nav" select="$workingNode/.." />
				</xsl:if>	
			</xsl:when>
		</xsl:choose>	
	</xsl:if>
	<xsl:if test="$locobjectid = '' " >
		<xsl:apply-templates mode="ritt.toc.front"  select="//tocfront[@linkend != '' ]" />
	</xsl:if>
	</navset> 
</xsl:template>


<xsl:template match="*" mode="ancestor.type" >
<xsl:choose>
	<xsl:when test="local-name(ancestor-or-self::tocpart) != '' and local-name(ancestor-or-self::tocchap) != ''">chapter-part</xsl:when>
	<xsl:when test="local-name(ancestor-or-self::tocsubpart) != '' ">tocsubpart</xsl:when>
	<xsl:otherwise><xsl:value-of select="local-name(ancestor-or-self::tocback|ancestor-or-self::tocfront|ancestor-or-self::tocpart|ancestor-or-self::tocchap)"/></xsl:otherwise>	
</xsl:choose>
</xsl:template>	

<xsl:template match="tocfront" mode="ritt.toc.front" >
	<xsl:param name="link"><xsl:value-of select="." /></xsl:param>	
	
	<xsl:if test="position() = 1" >
		<navcurrent /><sectiontitle>About</sectiontitle><navprev/><navnext><xsl:value-of select="@linkend" /></navnext>
		<nexttype><xsl:call-template name="typeFormat" /></nexttype>
	</xsl:if>
</xsl:template>	

<xsl:template name="typeFormat"	>
	<xsl:param name="link"><xsl:value-of select="." /></xsl:param>	
	<xsl:choose>
			<xsl:when test="$link = 'About' or $link = 'dedication' "><xsl:value-of select="$link" /></xsl:when>
			<xsl:otherwise>preface</xsl:otherwise>	
		</xsl:choose>
</xsl:template>	

<!-- ## 25/11/05 For sect2/3/4/5 link updated in match only -->
<xsl:template match="toclevel1" mode="ritt.nav">
	<navcurrent><xsl:value-of select="tocentry/@linkend" /></navcurrent>
<!--<xsl:message>in toclevel1 looking at  <xsl:value-of select="../tocentry[1]/@linkend" /> vs. <xsl:value-of select="../../tocentry[1]/@linkend" /></xsl:message>-->		
	<partid><xsl:value-of select="../ancestor::tocpart[last()]/tocentry[1]/@linkend" /></partid>
	<parttitle><xsl:value-of select="../ancestor::tocpart[last()]/tocentry[1]" /></parttitle>
	<chapterid><xsl:value-of select="../tocentry[1]/@linkend" /></chapterid>
	<chaptertitle><xsl:value-of select="../tocentry[1]" /></chaptertitle>
	<sectiontitle><xsl:value-of select="tocentry[1]" /></sectiontitle>
	<xsl:apply-templates mode="predicessor.node" select="."	 />	 		
	<xsl:apply-templates mode="sucessor.node" select="."	 />	 		
</xsl:template>
<!-- ## 25/11/05 For sect2 link updated in match only -->
<xsl:template match="toclevel2"  mode="ritt.nav">
	<navcurrent><xsl:value-of select="tocentry/@linkend" /></navcurrent>
<!--<xsl:message>in toclevel1 looking at  <xsl:value-of select="../tocentry[1]/@linkend" /> vs. <xsl:value-of select="../../tocentry[1]/@linkend" /></xsl:message>-->		
	<partid><xsl:value-of select="../../ancestor::tocpart[last()]/tocentry[1]/@linkend" /></partid>
	<parttitle><xsl:value-of select="../../ancestor::tocpart[last()]/tocentry[1]" /></parttitle>
	<chapterid><xsl:value-of select="../../tocentry[1]/@linkend" /></chapterid>
	<chaptertitle><xsl:value-of select="../../tocentry[1]" /></chaptertitle>
	<sectiontitle><xsl:value-of select="tocentry[1]" /></sectiontitle>
	<xsl:apply-templates mode="predicessor.node" select="."	 />	 		
	<xsl:apply-templates mode="sucessor.node" select="."	 />	 		
</xsl:template>		
<!-- ## 25/11/05 For sect3 link updated in match only 	-->	
<xsl:template match="toclevel3"  mode="ritt.nav">
	<navcurrent><xsl:value-of select="tocentry/@linkend" /></navcurrent>
<!--<xsl:message>in toclevel1 looking at  <xsl:value-of select="../tocentry[1]/@linkend" /> vs. <xsl:value-of select="../../tocentry[1]/@linkend" /></xsl:message>-->		
	<partid><xsl:value-of select="../../../ancestor::tocpart[last()]/tocentry[1]/@linkend" /></partid>
	<parttitle><xsl:value-of select="../../../ancestor::tocpart[last()]/tocentry[1]" /></parttitle>
	<chapterid><xsl:value-of select="../../../tocentry[1]/@linkend" /></chapterid>
	<chaptertitle><xsl:value-of select="../../../tocentry[1]" /></chaptertitle>
	<sectiontitle><xsl:value-of select="tocentry[1]" /></sectiontitle>
	<xsl:apply-templates mode="predicessor.node" select="."	 />	 		
	<xsl:apply-templates mode="sucessor.node" select="."	 />	 		
</xsl:template>
<!-- ## 25/11/05 For sect4 link updated in match only -->
<xsl:template match="toclevel4"  mode="ritt.nav">
	<navcurrent><xsl:value-of select="tocentry/@linkend" /></navcurrent>
<!--<xsl:message>in toclevel1 looking at  <xsl:value-of select="../tocentry[1]/@linkend" /> vs. <xsl:value-of select="../../tocentry[1]/@linkend" /></xsl:message>-->		
	<partid><xsl:value-of select="../../../../ancestor::tocpart[last()]/tocentry[1]/@linkend" /></partid>
	<parttitle><xsl:value-of select="../../../../ancestor::tocpart[last()]/tocentry[1]" /></parttitle>
	<chapterid><xsl:value-of select="../../../../tocentry[1]/@linkend" /></chapterid>
	<chaptertitle><xsl:value-of select="../../../../tocentry[1]" /></chaptertitle>
	<sectiontitle><xsl:value-of select="tocentry[1]" /></sectiontitle>
	<xsl:apply-templates mode="predicessor.node" select="."	 />	 		
	<xsl:apply-templates mode="sucessor.node" select="."	 />	 		
</xsl:template>		
<!-- ## 25/11/05 For sect5 link updated in match only -->
<xsl:template match="toclevel5"  mode="ritt.nav">
	<navcurrent><xsl:value-of select="tocentry/@linkend" /></navcurrent>
<!--<xsl:message>in toclevel1 looking at  <xsl:value-of select="../tocentry[1]/@linkend" /> vs. <xsl:value-of select="../../tocentry[1]/@linkend" /></xsl:message>-->		
	<partid><xsl:value-of select="../../../../../ancestor::tocpart[last()]/tocentry[1]/@linkend" /></partid>
	<parttitle><xsl:value-of select="../../../../../ancestor::tocpart[last()]/tocentry[1]" /></parttitle>
	<chapterid><xsl:value-of select="../../../../../tocentry[1]/@linkend" /></chapterid>
	<chaptertitle><xsl:value-of select="../../../../../tocentry[1]" /></chaptertitle>
	<sectiontitle><xsl:value-of select="tocentry[1]" /></sectiontitle>
	<xsl:apply-templates mode="predicessor.node" select="."	 />	 		
	<xsl:apply-templates mode="sucessor.node" select="."	 />	 		
</xsl:template>		


<xsl:template match="tocpart"  mode="ritt.nav">
<navcurrent><xsl:value-of select="tocentry[1]/@linkend" /></navcurrent>
<sectiontitle><xsl:value-of select="tocentry[1]" /></sectiontitle>	
<partid><xsl:value-of select="tocentry[1]/@linkend" /></partid>
<parttitle><xsl:value-of select="tocentry[1]" /></parttitle>
<chapterid></chapterid>
<chaptertitle></chaptertitle>
	<xsl:call-template name="tocpart.precessor"  />
	<xsl:call-template name="tocpart.sucessor"  />	
</xsl:template>		

<xsl:template match="tocsubpart"  mode="ritt.nav"	>
<navcurrent><xsl:value-of select="tocentry[1]/@linkend" /></navcurrent>
<sectiontitle><xsl:value-of select="tocentry[1]" /></sectiontitle>	
<partid><xsl:value-of select="ancestor::tocpart[last()]/tocentry[1]/@linkend" /></partid>
<parttitle><xsl:value-of select="ancestor::tocpart[last()]/tocentry[1]" /></parttitle>
<chapterid><xsl:value-of select="tocentry[1]/@linkend" /></chapterid>
<chaptertitle><xsl:value-of select="tocentry[1]" /></chaptertitle>	
	<xsl:call-template name="subpart.precessor"  />
	<xsl:call-template name="subpart.sucessor"  />	
</xsl:template>		


<xsl:template match="tocfront"  mode="ritt.nav"	>
<navcurrent><xsl:value-of select="@linkend" /></navcurrent>
<sectiontitle><xsl:value-of select="." /></sectiontitle>	
<partid></partid>
<parttitle></parttitle>
<chapterid><xsl:value-of select="@linkend" /></chapterid>
<chaptertitle><xsl:value-of select="." /></chaptertitle>	
	<xsl:call-template name="tocfront.precessor"  />	 		
	<xsl:call-template name="tocfront.sucessor"  />	 		
</xsl:template>		

<!-- start front successor templates -->
<xsl:template name="tocfront.sucessor"  	>
	<xsl:choose>
		<xsl:when test="following-sibling::tocfront"><navnext><xsl:value-of select="following-sibling::tocfront/@linkend"	/></navnext><nexttype><!-- **17-11-06** modification of frontmatter next button linking  --><xsl:choose><xsl:when test="substring(following-sibling::tocfront[1]/@linkend,1,2) = 'dd'">dedication</xsl:when><xsl:when test="substring(following-sibling::tocfront[1]/@linkend,1,2) = 'pr'">preface</xsl:when><xsl:otherwise><xsl:value-of select="following-sibling::tocfront[1]"/></xsl:otherwise></xsl:choose><!-- **17-11-06 older --><!--<xsl:call-template name="typeFormat" ><xsl:with-param name="link" select="following-sibling::tocfront[1]" /></xsl:call-template>--></nexttype></xsl:when>
		<!-- part with info case -->
		<xsl:when test="following-sibling::*[1]/@role = 'partintro'"	><xsl:call-template name="partintro.sucessor"  /></xsl:when>
		<!-- subpart with info case -->
		<xsl:when test="following-sibling::*[1]/tocsubpart/@role = 'partintro'"	><xsl:call-template name="subpartintro.sucessor"  /></xsl:when>
		<!-- chapter case -->
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocchap' "><navnext><xsl:value-of select="following-sibling::tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>

<!-- ## 25/11/05 For sect2 link Test -->
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocchap' "><navnext><xsl:value-of select="following-sibling::tocchap//toclevel2/tocentry/@linkend"	/></navnext></xsl:when>
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocchap' "><navnext><xsl:value-of select="following-sibling::tocchap//toclevel3/tocentry/@linkend"	/></navnext></xsl:when>
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocchap' "><navnext><xsl:value-of select="following-sibling::tocchap//toclevel4/tocentry/@linkend"	/></navnext></xsl:when>
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocchap' "><navnext><xsl:value-of select="following-sibling::tocchap//toclevel5/tocentry/@linkend"	/></navnext></xsl:when>


		<!-- part case -->
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocpart' "><xsl:call-template name="part.first"  /></xsl:when>
	</xsl:choose>	
</xsl:template>		


<xsl:template name="partintro.sucessor">
<navnext><xsl:value-of select="following-sibling::*[1]/tocentry/@linkend" /></navnext><nexttype>part</nexttype>
</xsl:template>		

<xsl:template name="subpartintro.sucessor" >
<!-- **23-11-06** updated for subpart navigation -->
<!-- **previous** <navnext><xsl:value-of select="following-sibling::*[1]/tocsubpart/tocentry/@linkend" /></navnext><nexttype>part</nexttype> -->
<navnext><xsl:value-of select="following-sibling::*[1]/tocentry/@linkend" /></navnext><nexttype>part</nexttype>
</xsl:template>
<!-- end front successor templates -->

<xsl:template name="tocfront.precessor">
	<xsl:choose>
		<xsl:when test="preceding-sibling::tocfront"><navprev><xsl:value-of select="preceding-sibling::tocfront[1]/@linkend"	/></navprev>
		<prevtype><!-- **17-11-06** modification of frontmatter previous button linking  --><xsl:choose><xsl:when test="substring(preceding-sibling::tocfront[1]/@linkend,1,2) = 'dd'">dedication</xsl:when><xsl:when test="substring(preceding-sibling::tocfront[1]/@linkend,1,2) = 'pr'">preface</xsl:when><xsl:otherwise><xsl:value-of select="preceding-sibling::tocfront[1]"/></xsl:otherwise></xsl:choose><!-- **17-11-06 older --><!--<xsl:call-template name="typeFormat" ><xsl:with-param name="link"  >
		<xsl:choose>
			<xsl:when test="substring(preceding-sibling::tocfront[1]/@linkend,2) = 'dd' ">dedication</xsl:when>
			<xsl:otherwise>preface</xsl:otherwise>	
		</xsl:choose></xsl:with-param></xsl:call-template>--></prevtype></xsl:when>
	<xsl:otherwise><navprev /><prevtype>About</prevtype></xsl:otherwise>		
	</xsl:choose>
</xsl:template>		

<xsl:template name="part.first"  >
	<xsl:choose>
		<!-- part with info case -->
		<xsl:when test="following-sibling::*[1]/@role = 'partintro'"	><xsl:call-template name="partintro.sucessor"  /></xsl:when>
		<!-- subpart with info case -->
		<!-- DRJ - Commented out because it created dead successors for non-existent content sections such as pt0001sp0001 -->
		<!--<xsl:when test="following-sibling::*[1]/tocsubpart/@role = 'partintro'"	><xsl:call-template name="subpartintro.sucessor"  /></xsl:when>-->
		<!-- part case -->
		<xsl:when test="following-sibling::*[1]/tocchap"><navnext><xsl:value-of select="following-sibling::tocpart/tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>	
		<xsl:when test="following-sibling::*[1]/tocsubpart/tocchap"><navnext><xsl:value-of select="following-sibling::tocpart/tocsubpart/tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>	
 
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocback' and following-sibling::*[1]/@linkend" ><navnext><xsl:value-of select="following-sibling::*[1]/@linkend"	/></navnext><nexttype>appendix</nexttype></xsl:when>
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocback' and following-sibling::*[1]/tocentry/@linkend" ><navnext><xsl:value-of select="following-sibling::*[1]/tocentry/@linkend"	/></navnext><nexttype>appendix</nexttype></xsl:when>

<!-- **04-12-06** appendix inside part without subsection -->
		<xsl:when test="following-sibling::*[1]/*[name(.)='tocback'][1]/@linkend"><navnext><xsl:value-of select="following-sibling::*[1]/*[name(.)='tocback'][1]/@linkend"	/></navnext><nexttype>appendix</nexttype></xsl:when>

<!-- **04-12-06** appendix inside part with subsection -->
		<xsl:when test="following-sibling::*[1]/*[name(.)='tocback'][1]/toclevel1"><navnext><xsl:value-of select="following-sibling::*[1]/*[name(.)='tocback'][1]/tocentry/@linkend" /></navnext><nexttype>appendix</nexttype></xsl:when>
		<xsl:otherwise><navnext /></xsl:otherwise>	

	</xsl:choose>	
</xsl:template>

<xsl:template mode="sucessor.node" match="toclevel1">
	<xsl:choose>
	<!-- section sibling case -->
		<xsl:when test="following-sibling::toclevel1"><navnext><xsl:value-of select="following-sibling::toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="sucessor.node" select=".." /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>	

<!-- ## 25/11/05 For sect2 link Test	-->
<xsl:template mode="sucessor.node" match="toclevel2">
	<xsl:choose>
		<xsl:when test="following-sibling::toclevel2"><navnext><xsl:value-of select="following-sibling::toclevel2/tocentry/@linkend" /></navnext></xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="sucessor.node" select=".." /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>
<!-- ## 25/11/05 For sect3 link Test -->
<xsl:template mode="sucessor.node" match="toclevel3">
	<xsl:choose>
		<xsl:when test="following-sibling::toclevel3"><navnext><xsl:value-of select="following-sibling::toclevel3/tocentry/@linkend" /></navnext></xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="sucessor.node" select=".." /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>		
<!-- ## 25/11/05 For sect4 link Test -->
<xsl:template mode="sucessor.node" match="toclevel4">
	<xsl:choose>
		<xsl:when test="following-sibling::toclevel4"><navnext><xsl:value-of select="following-sibling::toclevel4/tocentry/@linkend" /></navnext></xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="sucessor.node" select=".." /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>		
<!-- ## 25/11/05 For sect5 link Test -->
<xsl:template mode="sucessor.node" match="toclevel5">
	<xsl:choose>
		<xsl:when test="following-sibling::toclevel5"><navnext><xsl:value-of select="following-sibling::toclevel5/tocentry/@linkend" /></navnext></xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="sucessor.node" select=".." /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>		


<xsl:template mode="sucessor.node" match="tocchap">
	<xsl:choose>
	<!-- section sibling case -->
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocchap'" ><navnext><xsl:value-of select="following-sibling::tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>	
	<!-- **01-12-06** to navigate chapter section to part -->
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocpart'" ><xsl:call-template name="part.first"	/></xsl:when>	
	<!-- **01-12-06** to navigate chapter section to subpart -->
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocpart'" ><xsl:call-template name="subpartintro.sucessor" /></xsl:when>	

		<xsl:when test="local-name(following-sibling::*[1]) = 'tocsubpart'" ><navnext><xsl:value-of select="following-sibling::*[1]/tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>	

		<xsl:when test="local-name(following-sibling::*[1]) = 'tocback' and following-sibling::*[1]/@linkend" ><navnext><xsl:value-of select="following-sibling::*[1]/@linkend"	/></navnext><nexttype>appendix</nexttype></xsl:when>
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocback' and following-sibling::*[1]/tocentry/@linkend" ><navnext><xsl:value-of select="following-sibling::*[1]/tocentry/@linkend"	/></navnext><nexttype>appendix</nexttype></xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="sucessor.node" select=".." /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>	

<xsl:template mode="sucessor.node" match="tocpart">
	<xsl:choose>
	<!-- section sibling case -->
		<xsl:when test="following-sibling::tocpart"><xsl:call-template name="part.first"	/></xsl:when>	
	<!-- 24-11-06 match for color plates appears after part -->
		<xsl:when test="following-sibling::*[1][name(.)='tocchap']" ><navnext><xsl:value-of select="following-sibling::*[1]/toclevel1/tocentry/@linkend"/></navnext></xsl:when>	
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocback' and following-sibling::*[1]/@linkend" ><navnext><xsl:value-of select="following-sibling::*[1]/@linkend"	/></navnext><nexttype>appendix</nexttype></xsl:when>
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocback' and following-sibling::*[1]/tocentry/@linkend" ><navnext><xsl:value-of select="following-sibling::*[1]/tocentry/@linkend"	/></navnext><nexttype>appendix</nexttype></xsl:when>
		<xsl:otherwise><navnext /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>	

<xsl:template mode="sucessor.node" match="tocsubpart">
	<xsl:choose>
	<!-- section sibling case -->
		<xsl:when test="following-sibling::tocsubpart/@role = 'partintro' " ><xsl:call-template name="subpartintro.sucessor"  /></xsl:when>
	<!-- **27-11-06** To nav subpart without role partintro -->
		<xsl:when test="following-sibling::tocsubpart" ><navnext><xsl:value-of select="following-sibling::tocsubpart/tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocback' and following-sibling::*[1]/@linkend" ><navnext><xsl:value-of select="following-sibling::*[1]/@linkend"	/></navnext><nexttype>appendix</nexttype></xsl:when>
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocback' and following-sibling::*[1]/tocentry/@linkend" ><navnext><xsl:value-of select="following-sibling::*[1]/tocentry/@linkend"	/></navnext><nexttype>appendix</nexttype></xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="sucessor.node" select=".." /></xsl:otherwise>
	</xsl:choose>
</xsl:template>
<!-- end successor templates -->
<!-- begin predicessor templates -->

<xsl:template mode="predicessor.node" match="toclevel1">
	<xsl:choose>
	<!-- section sibling case -->
		<xsl:when test="preceding-sibling::toclevel1">
			<navprev><xsl:value-of select="preceding-sibling::toclevel1[1]/tocentry/@linkend" /></navprev>
		</xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="predicessor.node" select=".." /></xsl:otherwise>
	</xsl:choose>	
</xsl:template>	

<!-- ## 25/11/05 For sect2 link Test -->
<xsl:template mode="predicessor.node" match="toclevel2">
	<xsl:choose>
		<xsl:when test="preceding-sibling::toclevel2"><navprev><xsl:value-of select="preceding-sibling::toclevel2[1]/tocentry/@linkend"	/></navprev></xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="predicessor.node" select=".." /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>		
<!-- ## 25/11/05 For sect3 link Test -->
<xsl:template mode="predicessor.node" match="toclevel3">
	<xsl:choose>
		<xsl:when test="preceding-sibling::toclevel3"><navprev><xsl:value-of select="preceding-sibling::toclevel3[1]/tocentry/@linkend"	/></navprev></xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="predicessor.node" select=".." /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>	
<!-- ## 25/11/05 For sect4 link Test -->
<xsl:template mode="predicessor.node"   match="toclevel4"		>
	<xsl:choose>
		<xsl:when test="preceding-sibling::toclevel4"><navprev><xsl:value-of select="preceding-sibling::toclevel4[1]/tocentry/@linkend"	/></navprev></xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="predicessor.node" select=".." /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>	
<!-- ## 25/11/05 For sect5 link Test -->
<xsl:template mode="predicessor.node"   match="toclevel5"		>
	<xsl:choose>
		<xsl:when test="preceding-sibling::toclevel5"><navprev><xsl:value-of select="preceding-sibling::toclevel5[1]/tocentry/@linkend"	/></navprev></xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="predicessor.node" select=".." /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>

<xsl:template mode="predicessor.node" match="tocchap">
	<xsl:choose>
		<!-- 24-11-06 match for tocpart before tocchap -->
		<xsl:when test="preceding-sibling::*[1]/tocchap"><navprev><xsl:value-of select="preceding-sibling::*[1]/tocchap[last()]/toclevel1[last()]/tocentry/@linkend" /></navprev></xsl:when>
		<!-- no preceeding level 1 call template with chapter level parent -->
		<xsl:when test="preceding-sibling::*[1]/toclevel1"><navprev><xsl:value-of select="preceding-sibling::tocchap[1]/toclevel1[last()]/tocentry/@linkend" /></navprev></xsl:when>
		<xsl:when test="preceding-sibling::tocfront"><navprev><xsl:value-of select="preceding-sibling::tocfront[1]/@linkend"	/></navprev>
		<prevtype><xsl:call-template name="typeFormat" >
			<xsl:with-param name="link"  ><xsl:choose>
			<xsl:when test="substring(preceding-sibling::tocfront[1]/@linkend,2) = 'dd' ">dedication'</xsl:when>
			<xsl:otherwise>preface</xsl:otherwise>	
		</xsl:choose></xsl:with-param>
		</xsl:call-template></prevtype></xsl:when>
		<xsl:otherwise><xsl:apply-templates mode="predicessor.node" select=".." /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>

<xsl:template mode="predicessor.node" match="tocpart">
	<xsl:choose>
	<!-- section sibling case -->
		<xsl:when test="preceding-sibling::tocpart"><xsl:call-template name="prevpart.last"	/></xsl:when>	
<!-- **01-12-06** to navigate part section to chapter last section backward direction -->
		<xsl:when test="preceding-sibling::*[1][name(.)='tocchap']"><xsl:call-template name="tocpart.precessor" /></xsl:when>	

		<xsl:when test="preceding-sibling::tocfront"><navprev><xsl:value-of select="preceding-sibling::tocfront[1]/@linkend"	/></navprev>
		<prevtype><xsl:call-template name="typeFormat">
			<xsl:with-param name="link"><xsl:choose>
			<xsl:when test="substring(preceding-sibling::tocfront[1]/@linkend,2) = 'dd'">dedication'</xsl:when>
			<xsl:otherwise>preface</xsl:otherwise>	
		</xsl:choose></xsl:with-param>
		</xsl:call-template></prevtype></xsl:when>
		<xsl:otherwise><prevtype /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>	

<xsl:template mode="predicessor.node" match="tocsubpart">
	<xsl:choose>
	<!-- section sibling case -->
		<xsl:when test="preceding-sibling::tocsubpart"><xsl:call-template name="subpart.last"/></xsl:when>	
		<xsl:when test="preceding-sibling::tocchap"><navprev><xsl:value-of select="preceding-sibling::*[1]/toclevel1[last()]/tocentry/@linkend" /></navprev></xsl:when>
		<xsl:when test="local-name(preceding-sibling::*[1]) = 'tocentry'"><navprev><xsl:value-of select="preceding-sibling::*[1]/@linkend" /></navprev></xsl:when>	
		<xsl:otherwise><xsl:apply-templates mode="predicessor.node" select=".." /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>	

<xsl:template name="prevpart.last">
	<xsl:choose>
		<xsl:when test="preceding-sibling::*[1]/tocsubpart/tocchap/toclevel1"><navprev><xsl:value-of select="preceding-sibling::tocpart[1]/tocsubpart[last()]/tocchap[last()]/toclevel1[last()]/tocentry/@linkend"	/></navprev></xsl:when>
		<xsl:when test="preceding-sibling::*[1]/tocchap/toclevel1"><navprev><xsl:value-of select="preceding-sibling::tocpart[1]/tocchap[last()]/toclevel1[last()]/tocentry/@linkend"	/></navprev></xsl:when>
		</xsl:choose>	
</xsl:template>		

<xsl:template name="subpart.last">
	<xsl:choose>
		<xsl:when test="preceding-sibling::*[1]/tocchap/toclevel1">
			<navprev><xsl:value-of select="preceding-sibling::tocsubpart[1]/tocchap[last()]/toclevel1[last()]/tocentry/@linkend" /></navprev>
		</xsl:when>
	</xsl:choose>	
</xsl:template>		

<xsl:template name="tocpart.sucessor">
	<xsl:choose>
	<!-- part childern case -->
		<xsl:when test="./tocsubpart/@role = 'partintro'"><navnext><xsl:value-of select="./tocsubpart/tocentry/@linkend"	/></navnext></xsl:when>
		<xsl:when test="./toclevel1"><navnext><xsl:value-of select="./toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
		<xsl:when test="./tocchap/toclevel1"><navnext><xsl:value-of select="./tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
		
<!-- ## 25/11/05 For sect2 link Test -->
	<xsl:when test="./toclevel2"><navnext><xsl:value-of select="./toclevel2/tocentry/@linkend"	/></navnext></xsl:when>
	<xsl:when test="./tocchap/toclevel1/toclevel2"><navnext><xsl:value-of select="./tocchap/toclevel1/toclevel2/tocentry/@linkend"	/></navnext></xsl:when>

	<xsl:when test="./toclevel3"><navnext><xsl:value-of select="./toclevel3/tocentry/@linkend"	/></navnext></xsl:when>
	<xsl:when test="./tocchap/toclevel1/toclevel2/toclevel3"><navnext><xsl:value-of select="./tocchap/toclevel1/toclevel2/toclevel3/tocentry/@linkend"	/></navnext></xsl:when>

	<xsl:when test="./toclevel4"><navnext><xsl:value-of select="./toclevel4/tocentry/@linkend"	/></navnext></xsl:when>
	<xsl:when test="./tocchap/toclevel1/toclevel2/toclevel3/toclevel4"><navnext><xsl:value-of select="./tocchap/toclevel1/toclevel2/toclevel3/toclevel4/tocentry/@linkend"	/></navnext></xsl:when>

	<xsl:when test="./toclevel5"><navnext><xsl:value-of select="./toclevel5/tocentry/@linkend"	/></navnext></xsl:when>
	<xsl:when test="./tocchap/toclevel1/toclevel2/toclevel3/toclevel4/toclevel5"><navnext><xsl:value-of select="./tocchap/toclevel1/toclevel2/toclevel3/toclevel4/toclevel5/tocentry/@linkend"	/></navnext></xsl:when>



	<!-- subpart childern case -->
		<xsl:when test="./tocsubpart/tocchap/toclevel1"><navnext><xsl:value-of select="./tocsubpart/tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>

		<xsl:when test="./tocback"><navnext><xsl:value-of select="./tocback/tocentry/@linkend" /></navnext><nexttype>appendix</nexttype></xsl:when>

		<!-- part sibling case -->
		<xsl:when test="./following-sibling::*[1]/toclevel1"><navnext><xsl:value-of select="./following-sibling::tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
		<!-- part sibling cases -->
		<!-- check for part intro -->
		<!-- part with info case -->
		<xsl:when test="./following-sibling::*[1]/@role = 'partintro'"	><navnext><xsl:value-of select="./following-sibling::*[1]/tocentry/@linkend"	/></navnext><nexttype>part</nexttype></xsl:when>
		<xsl:when test="./following-sibling::*[1]/toclevel1"><navnext><xsl:value-of select="./following-sibling::tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
		<xsl:when test="./following-sibling::*[1]/tocchap/toclevel1"><navnext><xsl:value-of select="./following-sibling::tocpart/tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
	</xsl:choose>	
</xsl:template>		

<xsl:template name="subpart.sucessor"  	>
	<xsl:choose>
	<!-- part childern case -->
		<xsl:when test="./tocsubpart/@role = 'partintro'"><navnext><xsl:value-of select="./tocsubpart/tocentry/@linkend"	/></navnext></xsl:when>
		<xsl:when test="./toclevel1"><navnext><xsl:value-of select="./toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
		<xsl:when test="./tocchap/toclevel1"><navnext><xsl:value-of select="./tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
	<!-- subpart childern case -->
		<xsl:when test="./tocsubpart/tocchap/toclevel1"><navnext><xsl:value-of select="./tocsubpart/tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>

		<!-- part sibling case -->
		<xsl:when test="./following-sibling::*[1]/toclevel1"><navnext><xsl:value-of select="./following-sibling::tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
		<!-- part sibling cases -->
		<!-- check for part intro -->
		<!-- part with info case -->
		<xsl:when test="./following-sibling::*[1]/@role = 'partintro'"	><navnext><xsl:value-of select="./following-sibling::*[1]/tocentry/@linkend"	/></navnext><nexttype>part</nexttype></xsl:when>
		<xsl:when test="./following-sibling::*[1]/toclevel1"><navnext><xsl:value-of select="./following-sibling::tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
		<xsl:when test="./following-sibling::*[1]/tocchap/toclevel1"><navnext><xsl:value-of select="./following-sibling::tocpart/tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
		
		<!-- subpart sibling case -->
		<xsl:when test="../following-sibling::*[1]/toclevel1"><navnext><xsl:value-of select="../following-sibling::tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
		<!-- part sibling cases -->
		<!-- check for part intro -->
		<!-- part with info case -->
		<xsl:when test="../following-sibling::*[1]/@role = 'partintro'"	><navnext><xsl:value-of select="../following-sibling::*[1]/tocentry/@linkend"	/></navnext><nexttype>part</nexttype></xsl:when>
		<xsl:when test="../following-sibling::*[1]/toclevel1"><navnext><xsl:value-of select="../following-sibling::tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>
		<xsl:when test="../following-sibling::*[1]/tocchap/toclevel1"><navnext><xsl:value-of select="../following-sibling::tocpart/tocchap/toclevel1/tocentry/@linkend"	/></navnext></xsl:when>

	</xsl:choose>	
</xsl:template>		

<xsl:template name="tocback.sucessor"  	>
	<xsl:choose>
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocback' and following-sibling::*[1]/@linkend" ><navnext><xsl:value-of select="following-sibling::*[1]/@linkend"	/></navnext><nexttype>appendix</nexttype></xsl:when>
		<xsl:when test="local-name(following-sibling::*[1]) = 'tocback' and following-sibling::*[1]/tocentry/@linkend" ><navnext><xsl:value-of select="following-sibling::*[1]/tocentry/@linkend"	/></navnext><nexttype>appendix</nexttype></xsl:when>
		<xsl:when test="local-name(../following-sibling::*[1]) = 'tocback' and ../following-sibling::*[1]/@linkend" ><navnext><xsl:value-of select="../following-sibling::*[1]/@linkend"	/></navnext><nexttype>appendix</nexttype></xsl:when>
		<xsl:when test="local-name(../following-sibling::*[1]) = 'tocback' and ../following-sibling::*[1]/tocentry/@linkend" ><navnext><xsl:value-of select="../following-sibling::*[1]/tocentry/@linkend"	/></navnext><nexttype>appendix</nexttype></xsl:when>		
		<xsl:otherwise><navnext /></xsl:otherwise>	
	</xsl:choose>	
</xsl:template>		


<xsl:template name="tocpart.precessor"  	>
	<xsl:choose>
		<!-- no preceeding level 1 call template with chapter level parent -->
		<xsl:when test="preceding-sibling::*[1]/toclevel1"><navprev><xsl:value-of select="preceding-sibling::tocchap[1]/toclevel1[last()]/tocentry/@linkend"	/></navprev></xsl:when>
		<!-- chapter in other parts look at part first-->		
		<!-- chapter in other parts -->		
		<xsl:when test="preceding-sibling::*[1]/tocchap/toclevel1"><navprev><xsl:value-of select="preceding-sibling::tocpart[1]/tocchap[last()]/toclevel1[last()]/tocentry/@linkend"	/></navprev></xsl:when>
		<!-- chapter in other subparts -->		
		<xsl:when test="preceding-sibling::*[1]/tocsubpart/tocchap/toclevel1"><navprev><xsl:value-of select="preceding-sibling::tocpart[1]/tocsubpart[last()]/tocchap[last()]/toclevel1[last()]/tocentry/@linkend"	/></navprev></xsl:when>
		<!-- part with info cases -->
		<xsl:when test="preceding-sibling::*[1]/@role = 'partintro'"><navprev><xsl:value-of select="preceding-sibling::*[1]/tocentry/@linkend"	/></navprev><prevtype>part</prevtype></xsl:when>
		<!-- look for front parts -->
		<xsl:when test="preceding-sibling::tocfront"><navprev><xsl:value-of select="preceding-sibling::tocfront[1]/@linkend"	/></navprev>
		<prevtype><xsl:call-template name="typeFormat" ><xsl:with-param name="link"  select="preceding-sibling::tocfront[1]"	/></xsl:call-template></prevtype></xsl:when>
	<xsl:otherwise><navprev /><prevtype>About</prevtype></xsl:otherwise>		
	</xsl:choose>	
</xsl:template>		

<xsl:template name="subpart.precessor">
	<xsl:choose>
		<!-- no preceeding level 1 call template with chapter level parent -->
		<xsl:when test="preceding-sibling::*[1]/toclevel1"><navprev><xsl:value-of select="preceding-sibling::tocchap[1]/toclevel1[last()]/tocentry/@linkend"	/></navprev></xsl:when>
		<!-- chapter in other parts look at part first-->		
		<!-- chapter in other parts -->		
		<xsl:when test="preceding-sibling::*[1]/tocchap/toclevel1"><navprev><xsl:value-of select="preceding-sibling::tocpart[1]/tocchap[last()]/toclevel1[last()]/tocentry/@linkend"	/></navprev></xsl:when>
		<!-- part with info cases -->
		<xsl:when test="preceding-sibling::*[1]/@role = 'partintro'"><navprev><xsl:value-of select="preceding-sibling::*[1]/tocentry/@linkend"	/></navprev><prevtype>part</prevtype></xsl:when>
		<!-- look for front parts -->
		<xsl:when test="preceding-sibling::tocfront"><navprev><xsl:value-of select="preceding-sibling::tocfront[1]/@linkend"	/></navprev>
		<prevtype><xsl:call-template name="typeFormat" ><xsl:with-param name="link"  select="preceding-sibling::tocfront[1]"	/></xsl:call-template></prevtype></xsl:when>
		<!-- sub part cases -->
		<xsl:when test="preceding-sibling::tocentry[1]"><navprev><xsl:value-of select="preceding-sibling::tocentry[1]/@linkend"	/></navprev></xsl:when>
		<xsl:when test="../preceding-sibling::*[1]/toclevel1"><navprev><xsl:value-of select="../preceding-sibling::tocchap[1]/toclevel1[last()]/tocentry/@linkend"	/></navprev></xsl:when>
		<!-- chapter in other parts look at part first-->		
		<!-- chapter in other parts -->		
		<xsl:when test="../preceding-sibling::*[1]/tocchap/toclevel1"><navprev><xsl:value-of select="../preceding-sibling::tocpart[1]/tocchap[last()]/toclevel1[last()]/tocentry/@linkend"	/></navprev></xsl:when>
		<!-- part with info cases -->
		<xsl:when test="../preceding-sibling::*[1]/@role = 'partintro'"><navprev><xsl:value-of select="../preceding-sibling::*[1]/tocentry/@linkend"	/></navprev><prevtype>part</prevtype></xsl:when>
		<!-- look for front parts -->
		<xsl:when test="../preceding-sibling::tocfront"><navprev><xsl:value-of select="../preceding-sibling::tocfront[1]/@linkend"	/></navprev>
		<prevtype><xsl:call-template name="typeFormat" ><xsl:with-param name="link"  select="../preceding-sibling::tocfront[1]"	/></xsl:call-template></prevtype></xsl:when>

	<xsl:otherwise><navprev /><prevtype>About</prevtype></xsl:otherwise>		
	</xsl:choose>	
</xsl:template>		

<xsl:template name="tocback.precessor">
	<xsl:choose>
		<xsl:when test="local-name(preceding-sibling::*[1]) = 'tocback' " >
			<navprev>
			<xsl:choose>
				<xsl:when test="preceding-sibling::*[1]/@linkend " ><xsl:value-of select="preceding-sibling::*[1]/@linkend" /></xsl:when>
				<xsl:otherwise><xsl:value-of select="preceding-sibling::*[1]/tocentry/@linkend"	/></xsl:otherwise>		
			</xsl:choose>
			</navprev><prevtype>appendix</prevtype>
		</xsl:when>
		<!-- no preceeding level 1 call template with chapter level parent -->
		<xsl:when test="local-name(preceding-sibling::*[1]) = 'tocchap' "><navprev><xsl:value-of select="preceding-sibling::tocchap[1]/toclevel1[last()]/tocentry/@linkend"	/></navprev></xsl:when>
	  <!-- tocback preceded by tocpart/tocback -->		
		<xsl:when test="local-name(preceding-sibling::*[1]) = 'tocpart' and local-name(preceding-sibling::*[1]/child::*[last()]) = 'tocback'" ><navprev><xsl:value-of select="preceding-sibling::*[1]/tocback[last()]/tocentry/@linkend"	/></navprev><prevtype>appendix</prevtype></xsl:when>	
		<!-- chapter in other parts -->		
		<xsl:when test="local-name(preceding-sibling::*[1]) = 'tocpart' "><navprev><xsl:value-of select="preceding-sibling::tocpart[1]/tocchap[last()]/toclevel1[last()]/tocentry/@linkend"	/></navprev></xsl:when>
		<!-- part with info cases -->
		<xsl:when test="preceding-sibling::*[1]/@role = 'partintro'"><navprev><xsl:value-of select="preceding-sibling::*[1]/tocentry/@linkend"	/></navprev><prevtype>part</prevtype></xsl:when>
		<!-- look for front parts -->
		<xsl:when test="preceding-sibling::tocfront"><navprev><xsl:value-of select="preceding-sibling::tocfront[1]/@linkend"	/></navprev>
		<prevtype><xsl:call-template name="typeFormat" ><xsl:with-param name="link"  select="preceding-sibling::tocfront[1]"	/></xsl:call-template></prevtype></xsl:when>

		<xsl:when test="local-name(preceding-sibling::*[1]) = 'tocentry' and parent::tocpart[@role = 'partintro']">
			<navprev><xsl:value-of select="./preceding-sibling::tocentry[1]/@linkend"	/></navprev>
		</xsl:when>

		<!-- **04-12-06** for part last section -->
		<xsl:when test="local-name(preceding-sibling::*[1]) = 'tocentry' and parent::tocpart[preceding-sibling::*[1]/tocchap/toclevel1]">
			<navprev><xsl:value-of select="parent::tocpart/preceding-sibling::tocpart[1]/tocchap[last()]/toclevel1[last()]/tocentry/@linkend"	/></navprev>
		</xsl:when>

		<xsl:when test="local-name(preceding-sibling::*[1]) = 'tocentry' and parent::tocpart[preceding-sibling::tocchap[1]/toclevel1]">
			<navprev><xsl:value-of select="parent::tocpart/preceding-sibling::tocchap[1]/toclevel1[last()]/tocentry/@linkend"	/></navprev>
		</xsl:when>

		<xsl:otherwise><navprev /><prevtype>About</prevtype></xsl:otherwise>		
	</xsl:choose>	
</xsl:template>		

<xsl:template match="tocback"  mode="ritt.nav"	>
<xsl:variable name="current">
	<xsl:choose >
		<xsl:when test="@linkend != ''"><xsl:value-of select="@linkend"	/></xsl:when>
		<xsl:otherwise><xsl:value-of select="tocentry/@linkend"	/></xsl:otherwise>	
	</xsl:choose>
</xsl:variable>	
<xsl:variable name="currentTitle">
	<xsl:choose >
		<xsl:when test="tocentry/@linkend != ''"><xsl:value-of select="tocentry[1]"	/></xsl:when>
		<xsl:when test="tocentry[1] != ''"><xsl:value-of select="tocentry[1]"	/></xsl:when>
		<xsl:otherwise><xsl:value-of select="."/></xsl:otherwise>	
	</xsl:choose>
</xsl:variable>	

<navcurrent><xsl:value-of select="$current" /></navcurrent>
<sectiontitle><xsl:value-of select="$currentTitle" /></sectiontitle>	
<chaptertitle><xsl:value-of select="$currentTitle" /></chaptertitle>	
<chapterid><xsl:value-of select="$current" /></chapterid>

	<xsl:call-template name="tocback.precessor"  />
	<xsl:call-template name="tocback.sucessor"  />	
</xsl:template>		

<xsl:template match="*" mode="linkparent">
<!--<xsl:message>Ancestor type = <xsl:value-of select="$ancestorType"/> </xsl:message>-->	
<xsl:choose>
	<xsl:when test="$ancestorType = 'tocfront' and ancestor::tocfront/@linkend" ><!--<xsl:message>1</xsl:message>--><xsl:value-of select="ancestor::tocfront/@linkend"/></xsl:when>
	<xsl:when test="$ancestorType = 'tocback' and ancestor::tocback/@linkend" ><!--<xsl:message>2</xsl:message>--><xsl:value-of select="ancestor::tocback/@linkend"/></xsl:when>
	<xsl:when test="$ancestorType = 'tocfront' and ancestor::tocfront/tocentry/@linkend" ><!--<xsl:message>3</xsl:message>--><xsl:value-of select="ancestor::tocfront/tocentry/@linkend"/></xsl:when>
	<xsl:when test="$ancestorType = 'tocback' and ancestor::tocback/tocentry/@linkend" ><!--<xsl:message>4</xsl:message>--><xsl:value-of select="ancestor::tocback/tocentry/@linkend"/></xsl:when>
	<xsl:when test="ancestor::toclevel1[1] != ''" ><!--<xsl:message>5</xsl:message>--><xsl:value-of select="ancestor::toclevel1/tocentry/@linkend"/></xsl:when>
	<xsl:otherwise><!--<xsl:message>6<xsl:value-of select="./@linkend" /></xsl:message>--><xsl:value-of select="./@linkend" /></xsl:otherwise>	
</xsl:choose>	
</xsl:template>			
<xsl:template match="*"  mode="ritt.nav"></xsl:template>		
<xsl:template match="*" mode="sucessor.node"></xsl:template>		
<xsl:template match="*" mode="predicessor.node"></xsl:template>		

</xsl:stylesheet>