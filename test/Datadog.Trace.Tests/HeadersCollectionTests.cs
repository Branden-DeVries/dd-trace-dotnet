using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Net.Http;
using Datadog.Trace.ExtensionMethods;
using Datadog.Trace.Headers;
using Xunit;

namespace Datadog.Trace.Tests
{
    // TODO: for now, these tests cover all of this,
    // but we should probably split them up into actual *unit* tests for:
    // - HttpHeadersCollection wrapper over HttpHeaders (Get, Set, Add, Remove)
    // - NameValueHeadersCollection wrapper over NameValueCollection (Get, Set, Add, Remove)
    // - SpanContextPropagator.Inject()
    // - SpanContextPropagator.Extract()
    public class HeadersCollectionTests
    {
        [Fact]
        public void WebRequest_InjectExtract_Identity()
        {
            const int traceId = 9;
            const int spanId = 7;
            const SamplingPriority samplingPriority = SamplingPriority.UserKeep;
            const string origin = "synthetics";

            IHeadersCollection headers = WebRequest.CreateHttp("http://localhost").Headers.Wrap();
            var context = new SpanContext(traceId, spanId, samplingPriority, null, origin);

            SpanContextPropagator.Instance.Inject(context, headers);
            var resultContext = SpanContextPropagator.Instance.Extract(headers);

            Assert.NotNull(resultContext);
            Assert.Equal(context.SpanId, resultContext.SpanId);
            Assert.Equal(context.TraceId, resultContext.TraceId);
            Assert.Equal(context.SamplingPriority, resultContext.SamplingPriority);
            Assert.Equal(context.Origin, resultContext.Origin);
        }

        [Fact]
        public void WebRequest_ExtractHeaderTags_ReturnsEmptyWhenEmptyInput()
        {
            IHeadersCollection headers = WebRequest.CreateHttp("http://localhost").Headers.Wrap();
            var tagsFromHeader = SpanContextPropagator.Instance.ExtractHeaderTags(headers, new Dictionary<string, string>());

            Assert.NotNull(tagsFromHeader);
            Assert.Empty(tagsFromHeader);
        }

        [Fact]
        public void WebRequest_ExtractHeaderTags_MatchesCaseInsensitive()
        {
            // Initialize constants
            const string customHeader1Name = "dd-custom-header1";
            const string customHeader1Value = "match1";
            const string customHeader1TagName = "custom-header1-tag";

            const string customHeader2Name = "DD-CUSTOM-HEADER-MISMATCHING-CASE";
            const string customHeader2Value = "match2";
            const string customHeader2TagName = "custom-header2-tag";
            string customHeader2LowercaseHeaderName = customHeader2Name.ToLowerInvariant();

            // Initialize WebRequest and add headers
            HttpWebRequest webRequest = WebRequest.CreateHttp("http://localhost");
            webRequest.Headers.Add(customHeader1Name, customHeader1Value);
            webRequest.Headers.Add(customHeader2Name, customHeader2Value);

            // Initialize header tag arguments
            var headerTags = new Dictionary<string, string>();
            headerTags.Add(customHeader1Name, customHeader1TagName);
            headerTags.Add(customHeader2LowercaseHeaderName, customHeader2TagName);

            // Set expectations
            var expectedResults = new Dictionary<string, string>();
            expectedResults.Add(customHeader1TagName, customHeader1Value);
            expectedResults.Add(customHeader2TagName, customHeader2Value);

            // Test
            IHeadersCollection headers = webRequest.Headers.Wrap();
            var tagsFromHeader = SpanContextPropagator.Instance.ExtractHeaderTags(headers, headerTags);

            // Assert
            Assert.NotNull(tagsFromHeader);
            Assert.Equal(expectedResults, tagsFromHeader);
        }

        [Fact]
        public void NameValueCollection_InjectExtract_Identity()
        {
            const int traceId = 9;
            const int spanId = 7;
            const SamplingPriority samplingPriority = SamplingPriority.UserKeep;
            const string origin = "synthetics";

            IHeadersCollection headers = new NameValueCollection().Wrap();
            var context = new SpanContext(traceId, spanId, samplingPriority, null, origin);

            SpanContextPropagator.Instance.Inject(context, headers);
            var resultContext = SpanContextPropagator.Instance.Extract(headers);

            Assert.NotNull(resultContext);
            Assert.Equal(context.SpanId, resultContext.SpanId);
            Assert.Equal(context.TraceId, resultContext.TraceId);
            Assert.Equal(context.SamplingPriority, resultContext.SamplingPriority);
            Assert.Equal(context.Origin, resultContext.Origin);
        }

        [Fact]
        public void NameValueCollection_ExtractHeaderTags_ReturnsEmptyWhenEmptyInput()
        {
            IHeadersCollection headers = new NameValueCollection().Wrap();
            var tagsFromHeader = SpanContextPropagator.Instance.ExtractHeaderTags(headers, new Dictionary<string, string>());

            Assert.NotNull(tagsFromHeader);
            Assert.Empty(tagsFromHeader);
        }

        [Fact]
        public void NameValueCollection_ExtractHeaderTags_MatchesCaseInsensitive()
        {
            // Initialize constants
            const string customHeader1Name = "dd-custom-header1";
            const string customHeader1Value = "match1";
            const string customHeader1TagName = "custom-header1-tag";

            const string customHeader2Name = "DD-CUSTOM-HEADER-MISMATCHING-CASE";
            const string customHeader2Value = "match2";
            const string customHeader2TagName = "custom-header2-tag";
            string customHeader2LowercaseHeaderName = customHeader2Name.ToLowerInvariant();

            // Initialize WebRequest and add headers
            var nameValueCollection = new NameValueCollection();
            nameValueCollection.Add(customHeader1Name, customHeader1Value);
            nameValueCollection.Add(customHeader2Name, customHeader2Value);

            // Initialize header tag arguments
            var headerTags = new Dictionary<string, string>();
            headerTags.Add(customHeader1Name, customHeader1TagName);
            headerTags.Add(customHeader2LowercaseHeaderName, customHeader2TagName);

            // Set expectations
            var expectedResults = new Dictionary<string, string>();
            expectedResults.Add(customHeader1TagName, customHeader1Value);
            expectedResults.Add(customHeader2TagName, customHeader2Value);

            // Test
            IHeadersCollection headers = nameValueCollection.Wrap();
            var tagsFromHeader = SpanContextPropagator.Instance.ExtractHeaderTags(headers, headerTags);

            // Assert
            Assert.NotNull(tagsFromHeader);
            Assert.Equal(expectedResults, tagsFromHeader);
        }

        [Fact]
        public void DictionaryHeadersCollection_InjectExtract_Identity()
        {
            const int traceId = 9;
            const int spanId = 7;
            const SamplingPriority samplingPriority = SamplingPriority.UserKeep;
            const string origin = "synthetics";

            IHeadersCollection headers = new DictionaryHeadersCollection();
            var context = new SpanContext(traceId, spanId, samplingPriority, null, origin);

            SpanContextPropagator.Instance.Inject(context, headers);
            var resultContext = SpanContextPropagator.Instance.Extract(headers);

            Assert.NotNull(resultContext);
            Assert.Equal(context.SpanId, resultContext.SpanId);
            Assert.Equal(context.TraceId, resultContext.TraceId);
            Assert.Equal(context.SamplingPriority, resultContext.SamplingPriority);
            Assert.Equal(context.Origin, resultContext.Origin);
        }

        [Fact]
        public void DictionaryHeadersCollection_ExtractHeaderTags_ReturnsEmptyWhenEmptyInput()
        {
            IHeadersCollection headers = new DictionaryHeadersCollection();
            var tagsFromHeader = SpanContextPropagator.Instance.ExtractHeaderTags(headers, new Dictionary<string, string>());

            Assert.NotNull(tagsFromHeader);
            Assert.Empty(tagsFromHeader);
        }

        [Fact]
        public void DictionaryHeadersCollection_ExtractHeaderTags_MatchesCaseInsensitive()
        {
            // Initialize constants
            const string customHeader1Name = "dd-custom-header1";
            const string customHeader1Value = "match1";
            const string customHeader1TagName = "custom-header1-tag";

            const string customHeader2Name = "DD-CUSTOM-HEADER-MISMATCHING-CASE";
            const string customHeader2Value = "match2";
            const string customHeader2TagName = "custom-header2-tag";
            string customHeader2LowercaseHeaderName = customHeader2Name.ToLowerInvariant();

            // Initialize WebRequest and add headers
            var dictionaryHeadersCollection = new DictionaryHeadersCollection();
            dictionaryHeadersCollection.Add(customHeader1Name, customHeader1Value);
            dictionaryHeadersCollection.Add(customHeader2Name, customHeader2Value);

            // Initialize header tag arguments
            var headerTags = new Dictionary<string, string>();
            headerTags.Add(customHeader1Name, customHeader1TagName);
            headerTags.Add(customHeader2LowercaseHeaderName, customHeader2TagName);

            // Set expectations
            var expectedResults = new Dictionary<string, string>();
            expectedResults.Add(customHeader1TagName, customHeader1Value);
            expectedResults.Add(customHeader2TagName, customHeader2Value);

            // Test
            var tagsFromHeader = SpanContextPropagator.Instance.ExtractHeaderTags(dictionaryHeadersCollection, headerTags);

            // Assert
            Assert.NotNull(tagsFromHeader);
            Assert.Equal(expectedResults, tagsFromHeader);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("trace.id")]
        public void Extract_InvalidTraceId(string traceId)
        {
            const string spanId = "7";
            const string samplingPriority = "2";
            const string origin = "synthetics";

            var headers = InjectContext(traceId, spanId, samplingPriority, origin);
            var resultContext = SpanContextPropagator.Instance.Extract(headers);

            // invalid traceId should return a null context even if other values are set
            Assert.Null(resultContext);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("span.id")]
        public void Extract_InvalidSpanId(string spanId)
        {
            const ulong traceId = 9;
            const SamplingPriority samplingPriority = SamplingPriority.UserKeep;
            const string origin = "synthetics";

            var headers = InjectContext(
                traceId.ToString(CultureInfo.InvariantCulture),
                spanId,
                ((int)samplingPriority).ToString(CultureInfo.InvariantCulture),
                origin);

            var resultContext = SpanContextPropagator.Instance.Extract(headers);

            Assert.NotNull(resultContext);
            Assert.Equal(traceId, resultContext.TraceId);
            Assert.Equal(default(ulong), resultContext.SpanId);
            Assert.Equal(samplingPriority, resultContext.SamplingPriority);
            Assert.Equal(origin, resultContext.Origin);
        }

        [Theory]
        [InlineData("-2")]
        [InlineData("3")]
        [InlineData("sampling.priority")]
        public void Extract_InvalidSamplingPriority(string samplingPriority)
        {
            const ulong traceId = 9;
            const ulong spanId = 7;
            const string origin = "synthetics";

            var headers = InjectContext(
                traceId.ToString(CultureInfo.InvariantCulture),
                spanId.ToString(CultureInfo.InvariantCulture),
                samplingPriority,
                origin);

            var resultContext = SpanContextPropagator.Instance.Extract(headers);

            Assert.NotNull(resultContext);
            Assert.Equal(traceId, resultContext.TraceId);
            Assert.Equal(spanId, resultContext.SpanId);
            Assert.Null(resultContext.SamplingPriority);
            Assert.Equal(origin, resultContext.Origin);
        }

        private static IHeadersCollection InjectContext(string traceId, string spanId, string samplingPriority, string origin)
        {
            IHeadersCollection headers = new DictionaryHeadersCollection();
            headers.Add(HttpHeaderNames.TraceId, traceId);
            headers.Add(HttpHeaderNames.ParentId, spanId);
            headers.Add(HttpHeaderNames.SamplingPriority, samplingPriority);
            headers.Add(HttpHeaderNames.Origin, origin);
            return headers;
        }
    }
}
