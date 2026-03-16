<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:doc="http://nwalsh.com/xsl/documentation/1.0"
                exclude-result-prefixes="doc" version='1.0'>

<xsl:template match="personname" mode="bibliography.mode">
    <xsl:apply-templates mode="bibliomixed.mode" select="."	/>
    <xsl:value-of select="$biblioentry.item.separator"/>
</xsl:template>

<xsl:template name="cleanwords">
	<xsl:param name="wordtoclean"></xsl:param>
	<xsl:variable name="wordtoclean2"><xsl:value-of select="normalize-space($wordtoclean)" /></xsl:variable>	
	<xsl:variable name="wordlen"><xsl:value-of select="string-length($wordtoclean2)" /></xsl:variable>	
	<xsl:variable name="andwordlen">
		<xsl:choose>
			<xsl:when test="$wordlen &gt; 5"><xsl:value-of select="$wordlen - 5 " /></xsl:when>
			<xsl:otherwise>1</xsl:otherwise>	
		</xsl:choose>		
	</xsl:variable>	
	<xsl:choose>
		<xsl:when test="number($wordlen) = 1"><xsl:value-of select="$wordtoclean2"/></xsl:when>	
		<xsl:when test="substring-before( $wordtoclean2, '&amp;' ) != '' ">
		<xsl:call-template name="cleanwords2" ><xsl:with-param name="wordtoclean"><xsl:value-of select="normalize-space( concat(substring-before( $wordtoclean2, '&amp;' ), ' ' , substring-after( $wordtoclean2, '&amp;' ) ) )" /></xsl:with-param></xsl:call-template>	
	</xsl:when>
	<xsl:when test="substring-after(substring($wordtoclean2 , 1 , 5) , 'and ') != '' "><xsl:call-template name="cleanwords2" ><xsl:with-param name="wordtoclean"><xsl:value-of select="substring( $wordtoclean2 , 5, (number($wordlen) - 4) )" /></xsl:with-param></xsl:call-template></xsl:when>
		<xsl:when test="substring-before(substring($wordtoclean2, ($wordlen - 4), 5), ' and')  != '' "><xsl:call-template name="cleanwords2" ><xsl:with-param name="wordtoclean"><xsl:value-of select="substring($wordtoclean2, 1, (number($wordlen) - 4) )" /></xsl:with-param></xsl:call-template></xsl:when>
		<!-- ## 11/01/06 Removing comma from ", etal:" as comma and colon is auto inserted -->
	<xsl:when test="substring($wordtoclean2, 1, 8) = ', et al:' "><xsl:value-of select="substring($wordtoclean2, 3, 5)" /></xsl:when>
		<!-- ## 29/04/06 Removing comma from ", etal." as comma is auto inserted -->
	<xsl:when test="@role='etal' and substring($wordtoclean2, 1, 8) = ', et al.'"><xsl:value-of select="substring($wordtoclean2, 3)" /></xsl:when>
		<xsl:when test="substring(  $wordtoclean2 ,  number($wordlen)  , 1) = ':' "><xsl:value-of select="substring( $wordtoclean2 , 1, (number($wordlen) -1)  )" /></xsl:when>
		<xsl:when test="substring(  $wordtoclean2 ,  number($wordlen)  , 1) = ',' "><xsl:value-of select="substring( $wordtoclean2 , 1, (number($wordlen) -1)	 )" /></xsl:when> 
		<!-- ## 27/04/06 to remove extra comma apppearing at the start of the first name -->
		<xsl:when test="substring($wordtoclean2, 1, 1) = ',' and ancestor::editor[1][preceding-sibling::editor] and name(.) = 'firstname'"><xsl:value-of select="substring( $wordtoclean2 , 2, (number($wordlen) -1))" /></xsl:when>
		<xsl:when test="substring(  $wordtoclean2 ,  number($wordlen)  , 1) = ';' "><xsl:value-of select="substring( $wordtoclean2 , 1, (number($wordlen) -1)	 )" /></xsl:when> 
		<!-- ## 03/01/06 To remove 'and' from the last <EDITOR> where <firstname> has 'comma  and'  -->
		<xsl:when test="substring(  $wordtoclean2, 1, 5) = ', and' "><xsl:value-of select="substring( $wordtoclean2 , 1, 2)" /><xsl:value-of select="substring($wordtoclean2 , 6)" /></xsl:when> 
		<!-- ## 10/01/06 To remove '&' from the last <EDITOR> where <firstname> has '&amp;'  -->	
		<xsl:when test="substring(  $wordtoclean2, 1, 1) = '&amp;' "><xsl:value-of select="substring($wordtoclean2 , 2)" /></xsl:when> 		
	  <xsl:otherwise>
	    <xsl:choose>
	      <!-- ## If currently processing middle initial and NOT processing last author in authorgroup, add comma and space -->
				<!-- ## No longer do this after fixing DocBook -->
	      <!--<xsl:when test="self::node()[@role='mi'] != '' and not(ancestor::author = ancestor::authorgroup/author[last()])"><xsl:value-of select="$wordtoclean2"/><xsl:text>, </xsl:text></xsl:when>-->
	      
	      <!-- ## If 1) currently processing firstname, 2) there's no middle initial, and 3) NOT processing the last author in authorgroup, add comma and space -->
				<!-- ## No longer do this after fixing DocBook -->
	      <!--<xsl:when test="name(.) = 'firstname' and not(../othername[@role='mi']) and not(ancestor::author = ancestor::authorgroup/author[last()])"><xsl:value-of select="$wordtoclean2"/><xsl:text>, </xsl:text></xsl:when>-->

	      <!-- ## Add space after firstname prior to 'et al.', not space and comma... -->
	      <xsl:when test="name(.) = 'firstname' and (../othername[@role='etal']) and not(../othername[@role='mi'])"><xsl:value-of select="$wordtoclean2"/><xsl:text> </xsl:text></xsl:when>
	      
	      <!-- ## ..but not if there's a middle initial -->
	      <xsl:when test="name(.) = 'firstname' and (../othername[@role='etal']) and (../othername[@role='mi'])"><xsl:value-of select="$wordtoclean2"/></xsl:when>
	      
	      <!-- ## If 1) currently processing middle initial, and 2) there's a 'et al', then add a space -->
	      <xsl:when test="self::node()[@role='mi'] != '' and (../othername[@role='etal'])"><xsl:value-of select="$wordtoclean2"/><xsl:text> </xsl:text></xsl:when>

	      <!-- ## If 1) currently processing firstname, 2) there's no middle initial, and 3) processing the last author in authorgroup, add comma, space, and 'and' -->
	      <!--
        <xsl:when test="name(.) = 'surname' and ancestor::author = ancestor::authorgroup/author[last()]"><xsl:text>and </xsl:text><xsl:value-of select="$wordtoclean2"/></xsl:when>
        -->

	      <xsl:otherwise><xsl:value-of select="$wordtoclean2"/></xsl:otherwise>
	    </xsl:choose>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="cleanwords2">
	<xsl:param name="wordtoclean" ></xsl:param>
	<xsl:variable name="wordtoclean2"><xsl:value-of select="normalize-space($wordtoclean)" /></xsl:variable>	
	<xsl:variable name="wordlen"><xsl:value-of select="string-length($wordtoclean2)" /></xsl:variable>	
	<!-- ## 16/12/05 '=' is added after &gt; & space before, in otherwise to render the single char in firstname -->
	<xsl:if test="number($wordlen) &gt;=1">
	<xsl:choose>
		<xsl:when test="substring(  $wordtoclean2 ,  number($wordlen)  , 1 ) = ':' "><xsl:value-of select="substring( $wordtoclean2 , 1, (number($wordlen) -1)  )" /></xsl:when>
		<xsl:when test="substring(  $wordtoclean2 ,  number($wordlen)  , 1) = ',' "><xsl:value-of select="substring( $wordtoclean2 , 1, (number($wordlen) -1)	 )" /></xsl:when><!-- ## 16/12/05 space is added --><xsl:otherwise><!-- ## 14/09/06 extra space appearing before first name in case of panctuation --><xsl:if test="substring($wordtoclean2,1,1)!=','"><xsl:text> </xsl:text></xsl:if><xsl:value-of select="$wordtoclean2"/></xsl:otherwise>	
	</xsl:choose>				
	</xsl:if>
</xsl:template>	

<xsl:template match="emphasis" mode="ritt.plain.space"><xsl:text> </xsl:text><xsl:apply-templates mode="ritt.plain.space"	/><xsl:text> </xsl:text></xsl:template>	

<xsl:template match="*" mode="ritt.plain.space"><xsl:value-of select="."	/></xsl:template>

<xsl:template match="firstname" mode="bibliography.mode">
	<xsl:call-template name="cleanwords">
		<xsl:with-param name="wordtoclean"><xsl:apply-templates mode="ritt.plain.space" /></xsl:with-param></xsl:call-template>	
</xsl:template>

<xsl:template match="honorific|othername|lineage|degree" mode="bibliography.mode">
	<xsl:call-template name="cleanwords">
		<xsl:with-param name="wordtoclean"><xsl:apply-templates mode="ritt.plain.space" /></xsl:with-param>	
	</xsl:call-template>	
</xsl:template>

<xsl:template match="surname" mode="bibliography.mode">
   	<xsl:call-template name="cleanwords">
		<xsl:with-param name="wordtoclean" ><xsl:apply-templates mode="ritt.plain.space" /></xsl:with-param>	
	</xsl:call-template>	
</xsl:template>

<xsl:template match="personname" mode="bibliomixed.mode">
<!-- person is laid out in a fixed order which many of the books don't -->
    <xsl:if test="honorific[1] != '' " ><xsl:apply-templates mode="bibliomixed.mode" select="honorific[1]" />&#160;</xsl:if>
    <xsl:apply-templates mode="bibliomixed.mode" select="surname[1]" /><xsl:apply-templates mode="bibliomixed.mode" select="firstname[1]" />
    <xsl:if test="othername[@role = 'mi'] != '' ">&#160;<xsl:apply-templates mode="bibliomixed.mode" select="othername[@role = 'mi']" /></xsl:if>
    <xsl:if test="lineage[1] != '' " >&#160;<xsl:apply-templates mode="bibliomixed.mode" select="lineage[1]" /></xsl:if>
	  <xsl:if test="othername[@role = 'etal'] != '' ">&#160;</xsl:if>
    <xsl:if test="degree[1] != '' " >&#160;<xsl:apply-templates mode="bibliomixed.mode" select="degree[1]" /></xsl:if>
    <!-- ## 16/12/05 comma is added before 160; to render it before the etal -->
    <xsl:if test="othername[@role != 'mi'] != '' "><xsl:apply-templates mode="bibliomixed.mode" select="othername[@role != 'mi']" /></xsl:if>
</xsl:template>

<xsl:template match="surname" mode="bibliomixed.mode">
    	<xsl:call-template name="cleanwords">
		<xsl:with-param name="wordtoclean" ><xsl:apply-templates mode="ritt.plain.space" /></xsl:with-param>
	</xsl:call-template>
	<xsl:if test="../firstname != '' and ( substring-after(../firstname[1], ' ') = '' or substring-before(../firstname[1], ' ') = '' ) " >&#160;</xsl:if>
</xsl:template>

<xsl:template match="firstname" mode="bibliomixed.mode">
	<xsl:call-template name="cleanwords">
		<xsl:with-param name="wordtoclean"><xsl:apply-templates mode="ritt.plain.space" /></xsl:with-param>
	</xsl:call-template>	
</xsl:template>

<xsl:template match="honorific|othername|lineage|degree" mode="bibliomixed.mode">
	<xsl:call-template name="cleanwords">
		<xsl:with-param name="wordtoclean" ><xsl:apply-templates mode="ritt.plain.space" /></xsl:with-param>
	</xsl:call-template>	
</xsl:template>


<xsl:template match="author" mode="bibliomixed.mode">
    <xsl:apply-templates mode="bibliomixed.mode"/>
    <xsl:value-of select="$biblioentry.item.separator"/>
</xsl:template>

<xsl:template match="authorgroup" mode="bibliomixed.mode">
  <span class="{name(.)}">
    <xsl:call-template name="biblio.person.name.list" />
    <xsl:text>:  </xsl:text>
<!--    <xsl:value-of select="$biblioentry.item.separator"/>-->
  </span>
</xsl:template>

<xsl:template name="biblio.person.name.list">
  <!-- Return a formatted string representation of the contents of
       the current element. The current element must contain one or
       more AUTHORs, CORPAUTHORs, OTHERCREDITs, and/or EDITORs.

       John Doe
     or
       John Doe and Jane Doe
     or
       John Doe, Jane Doe, and A. Nonymous
  -->
  <xsl:param name="person.list" select="author|corpauthor|othercredit|editor"/>
  <xsl:param name="person.count" select="count($person.list)"/>
  <xsl:param name="count" select="1"/>
  <xsl:choose>
    <xsl:when test="$count &gt; $person.count"></xsl:when>
    <xsl:otherwise>
      <xsl:call-template name="biblio.person.name" >
        <xsl:with-param name="node" select="$person.list[position()=$count]"/>
      </xsl:call-template>

      <xsl:choose>
        <xsl:when test="$person.count = 2 and $count = 1">
             <xsl:call-template name="gentext.template" >
            <xsl:with-param name="context" select="'authorgroup'"/>
            <xsl:with-param name="name" select="'sep2'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$person.count &gt; 2 and $count+1 = $person.count">
          <xsl:call-template name="gentext.template" >
            <xsl:with-param name="context" select="'authorgroup'"/>
            <xsl:with-param name="name" select="'seplast'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$count &lt; $person.count">
          <xsl:call-template name="gentext.template">
            <xsl:with-param name="context" select="'authorgroup'"/>
            <xsl:with-param name="name" select="'sep'"/>
          </xsl:call-template>
        </xsl:when>
      </xsl:choose>

      <xsl:call-template name="biblio.person.name.list" >
        <xsl:with-param name="person.list" select="$person.list"/>
        <xsl:with-param name="person.count" select="$person.count"/>
        <xsl:with-param name="count" select="$count+1"/>
      </xsl:call-template>
    </xsl:otherwise>
  </xsl:choose>
  
  
</xsl:template><!-- person.name.list -->

<xsl:template name="biblio.person.name">
  <!-- Formats a personal name. Handles corpauthor as a special case. -->
  <xsl:param name="node" select="."/>

  <xsl:variable name="style">
    <xsl:choose>
      <xsl:when test="$node/@role">
        <xsl:value-of select="$node/@role"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="gentext.template">
          <xsl:with-param name="context" select="'styles'"/>
          <xsl:with-param name="name" select="'person-name'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:choose>
    <!-- the personname element is a specialcase -->
    <xsl:when test="$node/personname">
         <xsl:apply-templates select="$node/personname" mode="bibliomixed.mode"/>
    </xsl:when>

    <!-- handle corpauthor as a special case...-->
    <xsl:when test="name($node)='corpauthor'">
      <xsl:apply-templates select="$node" 	/>
    </xsl:when>

    <xsl:otherwise>
      <xsl:choose>
        <xsl:when test="$style = 'family-given'">
          <xsl:call-template name="person.name.family-given">
            <xsl:with-param name="node" select="$node"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$style = 'last-first'">
          <xsl:call-template name="person.name.last-first">
            <xsl:with-param name="node" select="$node"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="person.name.first-last">
            <xsl:with-param name="node" select="$node"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template match="bibliodiv">
    <xsl:apply-templates/>
</xsl:template>

<!-- Add other variable definitions here -->
<xsl:template match="biblioid" mode="bibliomixed.mode">
  <xsl:choose>
	  <xsl:when test="@otherclass = 'PubMedID' ">
		  <a target="_blank"><xsl:attribute name="href">https://www.ncbi.nlm.nih.gov/entrez/query.fcgi?cmd=Retrieve&amp;db=pubmed&amp;dopt=Abstract&amp;list_uids=<xsl:apply-templates mode="bibliomixed.mode"/></xsl:attribute>	<xsl:apply-templates mode="bibliomixed.mode"/></a>
		</xsl:when>
    <!-- <xsl:otherwise>HT
			  <xsl:apply-templates mode="bibliomixed.mode"/>
			  <xsl:value-of select="$biblioentry.item.separator"/>
		</xsl:otherwise> -->
  </xsl:choose>
</xsl:template>

<xsl:template match="pubdate" mode="bibliomixed.mode">
  <span class="pubdate">
		<xsl:choose>
			<xsl:when test="(substring(.,1,2) = '. ' or substring(.,1,2) = ', ' or substring(.,1,2) = ': ') and preceding-sibling::*[1][local-name(.)='authorgroup']">
				<xsl:value-of select="substring(.,3)" />
			</xsl:when>
			<xsl:when test="(substring(.,1,1) = '.' or substring(.,1,1) = ',' or substring(.,1,1) = ':') and preceding-sibling::*[1][local-name(.)='authorgroup']">
				<xsl:value-of select="substring(.,2)" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="." />
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text> </xsl:text>
	</span>
</xsl:template>

</xsl:stylesheet>