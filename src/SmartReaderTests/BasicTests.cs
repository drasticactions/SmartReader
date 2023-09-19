﻿using System;
using Xunit;
using SmartReader;
using System.IO;
using Xunit.Abstractions;
using AngleSharp.Html.Parser;
using RichardSzalay.MockHttp;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using System.Collections.Generic;

namespace SmartReaderTests
{
    public class BasicTests
    {
        private readonly ITestOutputHelper _output;
        public BasicTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestCleanTitleNoSitename()
        {
            Assert.Equal("Big title ", Readability.CleanTitle("Big title ", "Wikipedia"));
        }

        [Fact]
        public void TestCleanTitlePipe()
        {
            Assert.Equal("Big title", Readability.CleanTitle("Big title | Wikipedia", "Wikipedia"));
        }

        [Fact]
        public void TestCleanTitleBackslash()
        {
            Assert.Equal("Big title", Readability.CleanTitle("Big title / Wikipedia", "Wikipedia"));
        }

        [Fact]
        public void TestCleanTitleMark()
        {
            Assert.Equal("Big title", Readability.CleanTitle("Big title » Wikipedia", "Wikipedia"));
        }

        [Fact]
        public void TestCleanTitleNoSeparator()
        {
            Assert.Equal("Big title Wikipedia", Readability.CleanTitle("Big title Wikipedia", "Wikipedia"));
        }

        [Fact]
        public void TestCleanTitleNonStandardFormat()
        {
            Assert.Equal("Big title [ Wikipedia ]", Readability.CleanTitle("Big title [ Wikipedia ]", "Wikipedia"));
        }

        [Fact]
        public void TestCleanTitleDoesNotBreakWithRegexCharacters()
        {
            Assert.Equal("* No longer! *", Readability.CleanTitle("* No longer! *", "This is a *** problem"));
            Assert.Equal("Maybe ?", Readability.CleanTitle("Maybe ?", "Is this a problem?"));
        }

        [Fact]
        public void TestGetArticleTitleIdTitle()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head><title>An article with a complex idea</title></head>
               <body></body>
               </html>");

            Assert.Equal("An article with a complex idea", Readability.GetArticleTitle(doc));
        }

        [Fact]
        public void TestGetArticleTitleSeparator()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head><title>An article with a complex idea » By SomeSite</title></head>
               <body></body>
               </html>");

            Assert.Equal("An article with a complex idea", Readability.GetArticleTitle(doc));
        }

        [Fact]
        public void TestGetArticleTitleSeparatorNoSpace()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head><title>An article with a complex idea-error</title></head>
               <body></body>
               </html>");

            Assert.Equal("An article with a complex idea-error", Readability.GetArticleTitle(doc));
        }

        [Fact]
        public void TestGetArticleTitleSeparatorFewWords()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head><title>SomeSite - An  incredibly  smart title</title></head>
               <body></body>
               </html>");

            Assert.Equal("SomeSite - An incredibly smart title", Readability.GetArticleTitle(doc));
        }

        [Fact]
        public void TestGetArticleTitleSeparatorTooMuchWordsRemoved()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head><title>By SomeSite - An  incredibly  smart title</title></head>
               <body></body>
               </html>");

            Assert.Equal("By SomeSite - An incredibly smart title", Readability.GetArticleTitle(doc));
        }

        [Fact]
        public void TestGetArticleTitleColon()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head><title>SomeSite: An  incredibly  smart true title</title></head>
               <body></body>
               </html>");

            Assert.Equal("An incredibly smart true title", Readability.GetArticleTitle(doc));
        }

        [Fact]
        public void TestGetArticleTitleH1()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head><title>SomeSite</title></head>
               <body><h1>The right idea for you</h1></body>
               </html>");

            Assert.Equal("The right idea for you", Readability.GetArticleTitle(doc));
        }

        [Fact]
        public void TestGetMetadataDescription()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head>                   
                    <meta name=""og:description"" content=""The best article there is. Right here""/>
               </head>
               <body></body>
               </html>");

            Assert.Equal("The best article there is. Right here", Readability.GetArticleMetadata(doc, new Uri("https://localhost/"), "en", new Dictionary<string, string>()).Excerpt);
        }

        [Fact]
        public void TestGetMetadataSiteName()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head>                    
                    <meta name=""og:site_name"" content=""Some Good Site""/>
               </head>
               <body></body>
               </html>");

            Assert.Equal("Some Good Site", Readability.GetArticleMetadata(doc, new Uri("https://localhost/"), "en", new Dictionary<string, string>()).SiteName);
        }

        [Fact]
        public void TestGetMetadataTitle()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head>
                    <title>Some title</title>
                    <meta property=""twitter:title"" content=""Some Good Idea""/>
               </head>
               <body></body>
               </html>");

            Assert.Equal("Some Good Idea", Readability.GetArticleMetadata(doc, new Uri("https://localhost/"), "en", new Dictionary<string, string>()).Title);
        }

        [Fact]
        public void TestGetMetadataLanguage()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head>
                    <title>Some title</title>
                    <meta http-equiv=""Content-Language"" content=""it"">
               </head>
               <body></body>
               </html>");

            Assert.Equal("it", Readability.GetArticleMetadata(doc, new Uri("https://localhost/"), "", new Dictionary<string, string>()).Language);
        }

        [Fact]
        public void TestGetMetadataFeaturedImage()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head>
                    <meta name=""weibo:article:image"" content=""https://it.wikipedia.org/static/images/project-logos/itwiki-2x.png"">
               </head>
               <body></body>
               </html>");

            Assert.Equal("https://it.wikipedia.org/static/images/project-logos/itwiki-2x.png", Readability.GetArticleMetadata(doc, new Uri("https://localhost/"), "", new Dictionary<string, string>()).FeaturedImage);
        }

        [Fact]
        public void TestGetMetadataAuthor()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head>                    
                    <meta name=""author"" content=""Secret Man"">
               </head>
               <body></body>
               </html>");

            Assert.Equal("Secret Man", Readability.GetArticleMetadata(doc, new Uri("https://localhost/"), "", new Dictionary<string, string>()).Author);
        }

        [Fact]
        public void TestGetMetadataDateNoDate()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head></head>
               <body></body>
               </html>");

            Assert.Null(Readability.GetArticleMetadata(doc, new Uri("https://localhost/"), "", new Dictionary<string, string>()).PublicationDate);
        }

        [Fact]
        public void TestGetMetadataDateMeta()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head>                    
                    <meta itemprop=""datePublished"" content=""2110-10-21"" />
               </head>
               <body></body>
               </html>");

            Assert.Equal(new DateTime(2110, 10, 21), Readability.GetArticleMetadata(doc, new Uri("https://localhost/"), "", new Dictionary<string, string>()).PublicationDate);
        }

        [Fact]
        public void TestGetMetadataDateTimeTag()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head></head>
               <body><p>Hello. I am talking to you, <time datetime=""1980-09-01"" pubDate=""pubDate"">now</time></p></body>
               </html>");

            Assert.Equal(new DateTime(1980, 9, 1), Readability.GetArticleMetadata(doc, new Uri("https://localhost/"), "", new Dictionary<string, string>()).PublicationDate);
        }

        [Fact]
        public void TestGetMetadataDateUrl()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head></head>
               <body></body>
               </html>");

            Assert.Equal(new DateTime(2110, 10, 21), Readability.GetArticleMetadata(doc, new Uri("https://localhost/2110/10/21/"), "", new Dictionary<string, string>()).PublicationDate);
            Assert.Equal(new DateTime(2110, 10, 1), Readability.GetArticleMetadata(doc, new Uri("https://localhost/2110/10/37"), "", new Dictionary<string, string>()).PublicationDate);
            Assert.Equal(new DateTime(2010, 10, 1), Readability.GetArticleMetadata(doc, new Uri("https://localhost/2010/10/change_of_plans.html"), "", new Dictionary<string, string>()).PublicationDate);
        }

        [Fact]
        public void TestConvertImagesAsDataURI()
        {
            // creating element
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head></head>
               <body>
                    <p>This is a paragraph with some text.</p>
                    <p>This is a paragraph with some other text.</p>
                    <p>This is a paragraph with an image <img src=""https://localhost/small_image.png"" alt=""Nothing valuable""></img>.</p>
                    <p>This is a paragraph with an image <img src=""https://localhost/big_image.jpg"" alt=""Something very valuable""></img>.</p>
               </body>
               </html>");

            // setting up mocking HttpClient
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://localhost/small_image.png")
                    .Respond("image/png", File.OpenRead(@"..\..\..\test-images\small_image.png"));

            mockHttp.When("https://localhost/big_image.jpg")
                    .Respond("image/jpeg", File.OpenRead(@"..\..\..\test-images\big_image.jpg"));

            var reader = new Reader("https://localhost/article");

            Reader.SetBaseHttpClientHandler(mockHttp);

            var article = new Article(new Uri("https://localhost/article"), "Great article", "by Ulysses", "", "en", "Nobody", doc.Body, new Metadata(), true, reader);

            article.ConvertImagesToDataUriAsync().Wait();

            // check that there is one image
            Assert.Single(Regex.Matches(article.Content, "<img"));
            int start = article.Content.IndexOf("src=") + 4;
            int end = article.Content.IndexOf("\"", start + 1);
            // check that the src attribute is of the expected length
            Assert.Equal(572400, end - start);
        }

        [Fact]
        public void TestCheckSVGDataURIIsPreserved()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head></head>
               <body>
                    <p>This is a paragraph with some text.</p>
                    <p>This is a paragraph with some other text.</p>
                    <p>This is a paragraph with an image <img src=""data:image/svg+xml,%3C%3Fxml version='1.0' encoding='UTF-8'%3F%3E%3Csvg xmlns='http://www.w3.org/2000/svg' width='1' height='1'/%3E""></img>.</p>
               </body>
               </html>");

            Readability.FixRelativeUris(doc.Body, new Uri("https://localhost/article"), doc);

            if (doc.Body.GetElementsByTagName("img")[0].GetAttribute("src") is string src)
                Assert.False(src.StartsWith("https://localhost"));
        }

        [Fact]
        public void TestPlaintextConversion()
        {
            // creating element
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head></head>
               <body>
                    <p> </p>
                    <p>This is a paragraph with some text.</p>
                    <p>This  	 is a paragraph   with some other text and lots of whitespace  .</p>
                    <p>This is 			a paragraph with different<br> other text.</p>
               </body>
               </html>");

            var reader = new Reader("https://localhost/article");

            var article = new Article(new Uri("https://localhost/article"), "Great article", "by Ulysses", "", "en", "Nobody", doc.Body, new Metadata(), true, reader);

            // check that the text returned is correct
            Assert.Equal("This is a paragraph with some text.\r\n" +
                         "\r\nThis is a paragraph with some other text and lots of whitespace .\r\n" +
                         "\r\nThis is a paragraph with different\r\nother text.", article.TextContent);
        }

        [Fact]
        public void TestCustomSerializer()
        {
            // creating element
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head></head>
               <body>
                    <p> </p>
                    <p>This is a paragraph with some text.</p>
                    <p>This  	 is a paragraph   with some other text and lots of whitespace  .</p>
                    <p>This is 			a paragraph with different<br> other text.</p>
                    <pre>   Space inside here is       magic</pre>
               </body> 
               </html>");

            var reader = new Reader("https://localhost/article");

            static string serializer(IElement element)
            {
                return Regex.Replace(Regex.Replace(element.InnerHtml, @"(?<endBefore></.*?>)\s+(?<startAfter><[^/]>)", "${endBefore}${startAfter}"), @"(?<endBefore><(?!pre).*?>)\s+", "${endBefore}").Trim();
            }

            Article.Serializer = serializer;

            var article = new Article(new Uri("https://localhost/article"), "Great article", "by Ulysses", "", "en", "Nobody", doc.Body, new Metadata(), true, reader);

            // restore standard serializer
            Article.Serializer = (AngleSharp.Dom.IElement element) =>
            {
                return element.InnerHtml;
            };

            // check that the text returned is correct
            Assert.Equal(@"<p></p><p>This is a paragraph with some text.</p><p>This  	 is a paragraph   with some other text and lots of whitespace  .</p><p>This is 			a paragraph with different<br>other text.</p><pre>   Space inside here is       magic</pre>", article.Content);
        }

        [Fact]
        public void TestCustomConverter()
        {
            // creating element
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head></head>
               <body>
                    <p>JavaScript is a great language for system programming.</p>                   
               </body> 
               </html>");

            var reader = new Reader("https://localhost/article");

            static string converter(IElement element)
            {
                return element.TextContent.Replace("JavaScript", "**********").Trim();
            }

            var oldConverter = Article.Converter;

            Article.Converter = converter;

            var article = new Article(new Uri("https://localhost/article"), "Great article", "by Ulysses", "", "en", "Nobody", doc.Body, new Metadata(), true, reader);

            // check that the text returned is correct
            Assert.Equal(@"********** is a great language for system programming.", article.TextContent);

            // restore standard converter
            Article.Converter = oldConverter;
        }

        [Fact]
        public void TestGetMetadataAuthorFromJsonLD()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head>                    
                    <meta name=""author"" content=""Secret Man"">
					<script type=""application/ld+json"">
					{
						""@context"": ""http://schema.org""
						,""@type"": ""Article""						
						,""author"": {
                            ""@type"": ""Person"",
                            ""name"": ""Real Author""
                        }
					}
					</script>
               </head>
               <body></body>
               </html>");

            Assert.Equal("Real Author", Readability.GetArticleMetadata(doc, new Uri("https://localhost/"), "", Readability.GetJSONLD(doc)).Author);
        }

        [Fact]
        public void TestGetMetadataAuthorsFromJsonLD()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head>                    
                    <meta name=""author"" content=""Secret Man"">
					<script type=""application/ld+json"">
					{
						""@context"": ""http://schema.org""
						,""@type"": ""Article""						
						,""author"": [{
                            ""@type"": ""Person"",
                            ""name"": ""Real Author""
                        },{
                            ""@type"": ""Person"",
                            ""name"": ""Secret Man""
                        }]
					}
					</script>
               </head>
               <body></body>
               </html>");

            Assert.Equal("Real Author, Secret Man", Readability.GetArticleMetadata(doc, new Uri("https://localhost/"), "", Readability.GetJSONLD(doc)).Author);
        }

        [Fact]
        public void TestGetMetadataTypeOnlyCorrectJsonLDType()
        {
            var parser = new HtmlParser(new HtmlParserOptions());
            var doc = parser.ParseDocument(@"<html>
               <head>                    
                    <meta name=""author"" content=""Secret Man"">
					<script type=""application/ld+json"">
					{
						""@context"": ""http://schema.org""
						,""@type"": ""FakeArticle""
						,""author"": {
                            ""@type"": ""Person"",
                            ""name"": ""Real Author""
                        }
					}
					</script>
               </head>
               <body></body>
               </html>");

            Assert.Equal("Secret Man", Readability.GetArticleMetadata(doc, new Uri("https://localhost/"), "", Readability.GetJSONLD(doc)).Author);
        }
    }
}

