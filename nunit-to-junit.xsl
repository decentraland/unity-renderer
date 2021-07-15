<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" indent="yes" cdata-section-elements="system-out system-err" />

	<!-- NUnit2 results format -->
	<xsl:template match="/test-results">
	<testsuites>
		<xsl:for-each select="test-suite//results//test-case[1]">

			<xsl:for-each select="../..">
				<xsl:variable name="firstTestName"
					select="results/test-case[1]/@name" />
					<xsl:variable name="testFixtureName">
					<xsl:choose>
						<!-- we have a classic method name -->
						<xsl:when test="substring($firstTestName, string-length($firstTestName)) = ')'">
							<xsl:value-of select="substring-before($firstTestName, concat('.', @name))"></xsl:value-of>
						</xsl:when>

						<!-- we have either a custom name, or a test name -->
						<xsl:otherwise>
							<xsl:variable name="testMethodName">
								<xsl:call-template name="lastIndexOf">
									<xsl:with-param name="string" select="$firstTestName" />
									<xsl:with-param name="char"  select="'.'" />
								</xsl:call-template>
							</xsl:variable>

							<xsl:choose>
								<!-- If we didn't find any dot, it means we have just the test name -->
								<xsl:when test="$testMethodName=$firstTestName">
									<xsl:value-of select="concat(substring-before($firstTestName, @name), @name)" />
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="substring-before($firstTestName, concat('.', $testMethodName))" />
								</xsl:otherwise>
							</xsl:choose>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
                <!--
                <xsl:variable name="testFixtureName"
                    select="concat(substring-before($firstTestName, @name), @name)" />
                -->

				<!--  <redirect:write file="{$outputpath}/TEST-{$testFixtureName}.xml">-->

					<testsuite name="{$testFixtureName}"
						tests="{count(*/test-case)}" time="{@time}"
						failures="{count(*/test-case/failure)}" errors="0"
						skipped="{count(*/test-case[@executed='False' or @result='Inconclusive'])}">
						<xsl:for-each select="*/test-case">
							<xsl:variable name="testcaseName">
								<xsl:choose>
									<xsl:when test="contains(./@name, concat($testFixtureName,'.'))">
										<xsl:value-of select="substring-after(./@name, concat($testFixtureName,'.'))"/><!-- We either instantiate a "15" -->
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="./@name"/><!-- ...or a "20" -->
									</xsl:otherwise>
								</xsl:choose>
							</xsl:variable>

							<testcase classname="{$testFixtureName}"
								name="{$testcaseName}">
                                <xsl:if test="@time!=''">
                                   <xsl:attribute name="time"><xsl:value-of select="@time" /></xsl:attribute>
                                </xsl:if>

								<xsl:variable name="generalfailure"
									select="./failure" />

								<xsl:if test="./failure">
									<xsl:variable name="failstack"
				    						select="count(./failure/stack-trace/*) + count(./failure/stack-trace/text())" />
									<failure>
										<xsl:choose>
											<xsl:when test="$failstack &gt; 0 or not($generalfailure)">
MESSAGE:
<xsl:value-of select="./failure/message" />
+++++++++++++++++++
STACK TRACE:
<xsl:value-of select="./failure/stack-trace" />
											</xsl:when>
											<xsl:otherwise>
MESSAGE:
<xsl:value-of select="$generalfailure/message" />
+++++++++++++++++++
STACK TRACE:
<xsl:value-of select="$generalfailure/stack-trace" />
											</xsl:otherwise>
										</xsl:choose>
									</failure>
								</xsl:if>
                                <xsl:if test="@executed='False' or @result='Inconclusive'">
                                    <skipped>
                                    <xsl:attribute name="message"><xsl:value-of select="./reason/message"/></xsl:attribute>
                                    </skipped>
                                </xsl:if>
				 			</testcase>
						</xsl:for-each>
					</testsuite>
			</xsl:for-each>
		</xsl:for-each>
		</testsuites>
	</xsl:template>

	<!-- NUnit3 results format -->
	<xsl:template match="/test-run">
		<testsuites tests="{@testcasecount}" failures="{@failed}" disabled="{@skipped}" time="{@duration}">
			<xsl:apply-templates/>
		</testsuites>
	</xsl:template>

	<xsl:template match="test-suite">
		<xsl:if test="test-case">
			<testsuite tests="{@testcasecount}" time="{@duration}" errors="{@testcasecount - @passed - @skipped - @failed - @inconclusive}" failures="{@failed}" skipped="{@skipped + @inconclusive}" timestamp="{@start-time}">
				<xsl:attribute name="name">
					<xsl:for-each select="ancestor-or-self::test-suite/@name">
						<xsl:value-of select="concat(., '.')"/>
					</xsl:for-each>
				</xsl:attribute>
                <xsl:if test="output">
                    <xsl:apply-templates select="output" />
                </xsl:if>
				<xsl:apply-templates select="test-case"/>
			</testsuite>
			<xsl:apply-templates select="test-suite"/>
		</xsl:if>
		<xsl:if test="not(test-case)">
			<xsl:apply-templates/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="test-case">
		<testcase name="{@name}" assertions="{@asserts}" time="{@duration}" status="{@result}" classname="{@classname}">
			<xsl:if test="@runstate = 'Skipped' or @runstate = 'Ignored' or @runstate='Inconclusive'">
				<skipped/>
			</xsl:if>

			<xsl:apply-templates/>
		</testcase>
	</xsl:template>

	<xsl:template match="command-line"/>
	<xsl:template match="settings"/>

	<xsl:template match="output">
		<system-out>
			<xsl:copy-of select="./text()" />
		</system-out>
	</xsl:template>

	<xsl:template match="stack-trace">
	</xsl:template>

	<xsl:template match="test-case/failure">
		<failure message="{./message}">
			<xsl:value-of select="./stack-trace"/>
		</failure>
	</xsl:template>

	<xsl:template match="test-suite/failure"/>

	<xsl:template match="test-case/reason">
		<skipped message="{./message}"/>
	</xsl:template>

	<xsl:template match="test-suite/reason"/>


	<xsl:template match="properties"/>

	<!-- source: https://www.oxygenxml.com/archives/xsl-list/200102/msg00838.html -->
	<xsl:template name="lastIndexOf">
		<!-- declare that it takes two parameters - the string and the char -->
		<xsl:param name="string" />
		<xsl:param name="char" />

		<xsl:choose>
			<!-- if the string contains the character... -->
			<xsl:when test="contains($string, $char)">
				<xsl:call-template name="lastIndexOf">
					<xsl:with-param name="string"
									select="substring-after($string, $char)" />
					<xsl:with-param name="char" select="$char" />
				</xsl:call-template>
			</xsl:when>

			<xsl:otherwise>
				<xsl:value-of select="$string" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>