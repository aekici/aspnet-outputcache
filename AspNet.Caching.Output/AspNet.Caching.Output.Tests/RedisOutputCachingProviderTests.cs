using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;

using AspNet.Caching.Output.Providers;

using FluentAssertions;

using NUnit.Framework;

namespace AspNet.Caching.Output.Tests
{
    [TestFixture]
    public class RedisOutputCachingProviderTests
    {
        private readonly RedisOutputCachingProvider redisOutputCachingProvider = new RedisOutputCachingProvider();

        [TestFixtureSetUp]
        public void initialize_test_fixture()
        {
            var outputCacheSection = ConfigurationManager.GetSection("system.web/caching/outputCache") as OutputCacheSection;
            if (outputCacheSection == null)
            {
                throw new ConfigurationErrorsException("Output cache section is not defined!...");
            }

            if (outputCacheSection.Providers.Count == 0)
            {
                throw new ConfigurationErrorsException("No provider defined in system.web/caching/outputCache section!...");
            }

            string qualifiedName = Assembly.CreateQualifiedName(typeof(RedisOutputCachingProvider).Assembly.GetName().Name,
                                                                typeof(RedisOutputCachingProvider).FullName);
            ProviderSettings redisProviderSettings =
                outputCacheSection.Providers.Cast<ProviderSettings>().FirstOrDefault(provider => qualifiedName == provider.Type);

            if (redisProviderSettings == null)
            {
                throw new ConfigurationErrorsException(
                    "No RedisOutputCachingProvider defined in system.web/caching/outputCache section!...");
            }

            redisOutputCachingProvider.Initialize(outputCacheSection.DefaultProviderName, redisProviderSettings.Parameters);
        }

        [TestFixtureTearDown]
        public void tear_down_text_fixture()
        {
            redisOutputCachingProvider.Remove("this_is_now_in_cache");
            redisOutputCachingProvider.Remove("added_this_to_output_cache");
            redisOutputCachingProvider.Remove("fresh_new_item_to_update_later");
        }

        [Test]
        public void unavailable_cache_key_should_return_null()
        {
            object simpleObject = redisOutputCachingProvider.Get("this_shouldt_be_in_cache");
            simpleObject.Should().BeNull("Not added to cache any-time");
        }

        [Test]
        public void cached_item_should_be_returned()
        {
            redisOutputCachingProvider.Set("this_is_now_in_cache",
                                           new CachedItem { Key = "this_is_now_in_cache", Value = "you're right" },
                                           DateTime.Now.AddMinutes(5));
            var item = (CachedItem)redisOutputCachingProvider.Get("this_is_now_in_cache");

            item.Should().NotBeNull();
            item.Key.Should().BeEquivalentTo("this_is_now_in_cache");
            item.Value.ToString().Should().BeEquivalentTo("you're right");
        }

        [Test]
        public void an_item_should_be_added_to_cache()
        {
            redisOutputCachingProvider.Add("added_this_to_output_cache",
                                           new CachedItem { Key = "this_is_now_in_cache", Value = "Hooray, we're on track" },
                                           DateTime.Now.AddMinutes(5));
            var item = (CachedItem)redisOutputCachingProvider.Get("added_this_to_output_cache");

            item.Should().NotBeNull();
            item.Key.Should().BeEquivalentTo("this_is_now_in_cache");
            item.Value.ToString().Should().BeEquivalentTo("Hooray, we're on track");
        }

        [Test]
        public void an_existing_item_should_be_updated_on_cache()
        {
            redisOutputCachingProvider.Add("fresh_new_item_to_update_later",
                                           new CachedItem
                                               {
                                                   Key = "fresh_new_item_to_update_later",
                                                   Value = "Fallulah Bridges and Kids With Guns"
                                               }, DateTime.Now.AddMinutes(5));
            var item = (CachedItem)redisOutputCachingProvider.Get("fresh_new_item_to_update_later");

            item.Should().NotBeNull();
            item.Key.Should().BeEquivalentTo("fresh_new_item_to_update_later");
            item.Value.ToString().Should().BeEquivalentTo("Fallulah Bridges and Kids With Guns");

            //now update existing item
            redisOutputCachingProvider.Add("fresh_new_item_to_update_later",
                                           new CachedItem
                                               {
                                                   Key = "fresh_new_item_to_update_later",
                                                   Value = "Fallulah Give Us A Little Love"
                                               }, DateTime.Now.AddMinutes(5));
            item = (CachedItem)redisOutputCachingProvider.Get("fresh_new_item_to_update_later");

            item.Should().NotBeNull();
            item.Key.Should().BeEquivalentTo("fresh_new_item_to_update_later");
            item.Value.ToString().Should().BeEquivalentTo("Fallulah Give Us A Little Love");
        }

        [Test]
        public void an_existing_item_should_be_removed()
        {
            redisOutputCachingProvider.Add("new_item_to_be_removed_later",
                                           new CachedItem
                                               {
                                                   Key = "new_item_to_be_removed_later",
                                                   Value = "this to be removed in a second"
                                               }, DateTime.Now.AddMinutes(5));
            redisOutputCachingProvider.Remove("new_item_to_be_removed_later");

            var item = (CachedItem)redisOutputCachingProvider.Get("new_item_to_be_removed_later");
            item.Should().BeNull();
        }
    }
    [Serializable]
    public class CachedItem
    {
        private object obj = new System.Web.Caching.HeaderElement("ohannes", "burger");

        public string Key { get; set; }

        public object Value { get; set; }

        public object Obj
        {
            get { return obj; }
            set { obj = value; }
        }
    }
}
