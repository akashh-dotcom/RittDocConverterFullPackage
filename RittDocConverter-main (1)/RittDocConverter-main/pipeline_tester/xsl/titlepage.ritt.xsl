<?xml version='1.0'?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    version='1.0'>
    <!-- titlepage_ritt.xsl 
Contains extentions for section title pages and modifications for author styles that didn't quite work
-->
    <!-- author styles -->
    
    <!-- ## 14/02/06 Added for authorgroup within the partinfo tag -->
    <xsl:template match="authorgroup" mode="part.titlepage.recto.auto.mode">
        <xsl:call-template name="Author-formatting"><xsl:with-param name="TitleStyle">docsect1Title</xsl:with-param></xsl:call-template>    
    </xsl:template> 
    
    <xsl:template match="authorgroup" mode="sect1.titlepage.recto.auto.mode">
        <xsl:call-template name="Author-formatting"><xsl:with-param name="TitleStyle">docsect1Title</xsl:with-param></xsl:call-template>    
    </xsl:template>
    
    <!-- Added for glossary author 17-11-09 -->
    <xsl:template match="authorgroup" mode="glossary.titlepage.recto.auto.mode">
        <xsl:call-template name="Author-formatting"><xsl:with-param name="TitleStyle">docsect1Title</xsl:with-param></xsl:call-template>    
    </xsl:template>

    <!-- ## 3/4/2013 Added for authorgroup within the prefaceinfo tag -->
    <xsl:template match="authorgroup" mode="preface.titlepage.recto.mode"> 
        <xsl:call-template name="Author-formatting" />    
    </xsl:template>

    <xsl:template match="authorgroup" mode="sect2.titlepage.recto.auto.mode">
        <xsl:call-template name="Author-formatting"><xsl:with-param name="TitleStyle">docsect2Title</xsl:with-param></xsl:call-template>    
    </xsl:template>
    
    <xsl:template match="authorgroup" mode="sect3.titlepage.recto.auto.mode">
        <xsl:call-template name="Author-formatting"><xsl:with-param name="TitleStyle">docsect3Title</xsl:with-param></xsl:call-template>    
    </xsl:template>
    
    <xsl:template match="authorgroup" mode="sect4.titlepage.recto.auto.mode">
        <xsl:call-template name="Author-formatting"><xsl:with-param name="TitleStyle">docsect4Title</xsl:with-param></xsl:call-template>    
    </xsl:template>
    
    <xsl:template match="authorgroup" mode="sect5.titlepage.recto.auto.mode">
        <xsl:call-template name="Author-formatting"><xsl:with-param name="TitleStyle">docsect5Title</xsl:with-param></xsl:call-template>    
    </xsl:template>

	<!-- ## 9/2/2013 Added for authorgroup within the appendixinfo tag -->
	<xsl:template match="authorgroup" mode="appendix.titlepage.recto.auto.mode">
		<xsl:call-template name="Author-formatting">
			<xsl:with-param name="TitleStyle">docsect1Title</xsl:with-param>
		</xsl:call-template>
	</xsl:template>
    
    <xsl:template name="Author-formatting">
        <xsl:param name="TitleStyle">docsect1Style</xsl:param>  
        <xsl:variable name="authornames"> <xsl:value-of select="." /> </xsl:variable>
        <div xsl:use-attribute-sets="sect1.titlepage.recto.style">
            <!--<span><xsl:attribute name="class"><xsl:value-of select="$TitleStyle" /></xsl:attribute>-->
            <xsl:for-each select="*">
                <xsl:call-template name="Author-name-formatting" >
                    <xsl:with-param name="locposition" select="position()" />   
                    <xsl:with-param name="loclast" select="last()" />   
                    <xsl:with-param name="authorname"><!-- ORI MATCH <xsl:text> </xsl:text><xsl:apply-templates select="." mode="authorgroup.mode"  /><xsl:text> </xsl:text></xsl:with-param> -->
                        <!-- ## 29/11/2005 give space only when first char is not comma -->
                        <xsl:if test="substring(normalize-space(.),1,1) != ','">
                            <xsl:text> </xsl:text>
                        </xsl:if>

                            <xsl:apply-templates select="." mode="authorgroup.mode"/>
                        
                        <xsl:choose>
                            <xsl:when test="substring(normalize-space(following::text()[1]),1,1) != ','"></xsl:when>
                            <xsl:otherwise>
                                <xsl:text> </xsl:text>
                            </xsl:otherwise>
                        </xsl:choose>
                    </xsl:with-param>
                </xsl:call-template>
            </xsl:for-each>
            <!--</span>-->  
        </div>
    </xsl:template>     
    
    <xsl:template name="Author-name-formatting">
        <xsl:param name="locposition">0</xsl:param>
        <xsl:param name="loclast">0</xsl:param>
        <xsl:param name="authorname">
            <xsl:value-of select="."/>
        </xsl:param>
        
        <xsl:choose>

            <xsl:when test="$locposition &gt; 1 and $locposition != $loclast">
                <!-- ## 08/11/05 Insert comma within 2 authors only when preceding node doesn't have comma ## -->
                <xsl:if
                    test="substring(normalize-space(preceding::*[1]),number(string-length(normalize-space(preceding::*[1]))),1) != ','">
                    <!-- ## 28/11/05 Insert comma within 2 authors only when self node doesn't have comma ## -->
                    <xsl:if
                        test="substring(normalize-space(self::*[1]),number(string-length(normalize-space(self::*[1]))),1) != ','">
                        <!-- ## 29/11/05 Insert comma within 2 authors only when self node doesn't have comma ## -->
                        <xsl:if test="substring(normalize-space(.),1,1) != ','">, </xsl:if>
                    </xsl:if>
                </xsl:if>

            </xsl:when>
            <xsl:when test="$locposition &gt; 1 and substring-before( translate($authorname,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'), ' and '  ) = '' and substring-after( $authorname, ' and '  ) = ''"> and </xsl:when>
        </xsl:choose>
        <!-- ## 28/11/05 remove comma if its a last author as "and" is inserted above -->
        <xsl:choose>

            <xsl:when test="substring(normalize-space($authorname),1,1) = ',' and $locposition = $loclast">
                <!-- ## 14/12/05 ORI <xsl:value-of select="substring-after($authorname,',')" /> -->
                <!-- ## 14/12/05 For Adding superscript content below is new mode ##lastaut.mode## -->
                <xsl:apply-templates mode="lastaut.mode"/>
            </xsl:when>
            <!-- ## 13/01/06 New match for finding comma and & in second last author to remove it -->

            <xsl:when test="contains(normalize-space($authorname),', &amp;') and $locposition != $loclast">
                <xsl:value-of select="substring-before($authorname,', &amp;')"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$authorname"/>

                <!-- ## 02/12/05 For Adding superscript content which is deleted in common.ritt.xsl -->
                <!-- ## 28/12/05 if added to apply the sup only when present -->
                <xsl:if test=".//superscript">
                    <sup>
                        <xsl:value-of select=".//superscript"/>
                    </sup>
                </xsl:if>
            </xsl:otherwise>
        </xsl:choose>

        <!-- ## 01/12/05 to add address within the chapterinfo -->
        <xsl:if test="./address">
            <xsl:apply-templates select="./address"/>
        </xsl:if>

        <!-- ## 30/12/05 to add AFFILIATION within the chapterinfo though address is not parent Find html\verbatim.xsl for the new match for the same.  ## 11/07/06 " or parent::author" added -->

        <xsl:if test="./affiliation[not(ancestor::address or parent::author)]">
            <xsl:apply-templates select="./affiliation"/>
        </xsl:if>
    </xsl:template>

    <xsl:template match="author|editor|corpauthor|othercredit" mode="sect1.titlepage.recto.auto.mode">
        <!--<div xsl:use-attribute-sets="sect1.titlepage.recto.style" >-->
        <h3 class="author"><xsl:call-template name="person.name.first-last" /></h3>
        <!--</div>-->       
    </xsl:template>
    

    
    <xsl:template match="author|editor|corpauthor|othercredit" mode="authorgroup.mode"><xsl:call-template name="person.name.first-last" /></xsl:template>       
    
    <xsl:template match="*" mode="authorgroup.mode"><xsl:text> </xsl:text><xsl:apply-templates mode="authorgroup.mode"  /></xsl:template>           
    
    <!-- ## 14/12/05 Match for last author with superscript to show sup if present -->
    <xsl:template match="*" mode="lastaut.mode">
        <xsl:param name="locposition">0</xsl:param> 
        <xsl:param name="loclast">0</xsl:param> 
        <xsl:param name="authorname"><xsl:apply-templates /></xsl:param>
        <xsl:if test="substring(normalize-space($authorname),1,1) = ',' and $locposition = $loclast">
            <!--<xsl:call-template name="lastautsup"/> in common.ritt.xsl <xsl:value-of select="substring-after($authorname,',')" /> -->
            <xsl:call-template name="lastautsup">
                <xsl:with-param name="authorname" select="substring-after($authorname,',')" /> 
            </xsl:call-template>
            <sup><xsl:value-of select=".//superscript"/></sup>
        </xsl:if>
    </xsl:template>
    
    <!-- extentions for section title pages -->
    
    <xsl:attribute-set name="sect6.titlepage.recto.style"
        use-attribute-sets="section.titlepage.recto.style"/>
    
    
    <xsl:attribute-set name="sect6.titlepage.recto.style"
        use-attribute-sets="section.titlepage.recto.style"/>
    <xsl:attribute-set name="sect6.titlepage.verso.style"
        use-attribute-sets="section.titlepage.verso.style"/>
    
    <xsl:attribute-set name="sect7.titlepage.recto.style"
        use-attribute-sets="section.titlepage.recto.style"/>
    <xsl:attribute-set name="sect7.titlepage.verso.style"
        use-attribute-sets="section.titlepage.verso.style"/>
    
    <xsl:attribute-set name="sect8.titlepage.recto.style"
        use-attribute-sets="section.titlepage.recto.style"/>
    <xsl:attribute-set name="sect8.titlepage.verso.style"
        use-attribute-sets="section.titlepage.verso.style"/>
    
    <xsl:attribute-set name="sect9.titlepage.recto.style"
        use-attribute-sets="section.titlepage.recto.style"/>
    <xsl:attribute-set name="sect9.titlepage.verso.style"
        use-attribute-sets="section.titlepage.verso.style"/>
    
    <xsl:attribute-set name="sect10.titlepage.recto.style"
        use-attribute-sets="section.titlepage.recto.style"/>
    <xsl:attribute-set name="sect10.titlepage.verso.style"
        use-attribute-sets="section.titlepage.verso.style"/>
</xsl:stylesheet>
