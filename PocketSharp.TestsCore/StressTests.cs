﻿using PocketSharp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PocketSharp.Tests
{
    public class StressTests : TestsBase
    {
        private static IEnumerable<string> urls;
        private static string[] tags = new string[] { "css", "js", "csharp", "windows", "microsoft" };

        public StressTests()
          : base()
        {
            // !! please don't misuse this account !!
            client = new PocketClient(
              consumerKey: "20000-786d0bc8c39294e9829111d6",
              callbackUri: "http://frontendplay.com",
              accessCode: "9b8ecb6b-7801-1a5c-7b39-2ba05b"
            );

            urls = File.ReadAllLines("./url-100000.csv").Select(item => item.Split(',')[1]);
        }

        [Fact]
        public async Task Are100ItemsRetrievedProperly()
        {
            IEnumerable<PocketItem> items = await client.Get(count: 100, state: State.all);
            Assert.True(items.Count() == 100);
        }

        [Fact]
        public async Task Are1000ItemsRetrievedProperly()
        {
            IEnumerable<PocketItem> items = await client.Get(count: 1000, state: State.all);
            Assert.True(items.Count() == 1000);
        }

        [Fact]
        public async Task Are2500ItemsRetrievedProperly()
        {
            IEnumerable<PocketItem> items = await client.Get(count: 2500, state: State.all);
            Assert.True(items.Count() == 2500);
        }

        [Fact]
        public async Task Are5000ItemsRetrievedProperly()
        {
            IEnumerable<PocketItem> items = await client.Get(count: 5000, state: State.all);
            Assert.True(items.Count() == 5000);
        }

        [Fact]
        public async Task IsSearchSuccessfullyOnBigList()
        {
            IEnumerable<PocketItem> items = await client.Get(search: "google");

            Assert.True(items.Count() > 0);
            Assert.True(items.First().Title.ToLower().Contains("google") || items.First().Uri.ToString().ToLower().Contains("google"));
        }

        [Fact]
        public async Task IsTagSearchSuccessfullyOnBigList()
        {
            IEnumerable<PocketItem> items = await client.SearchByTag(tags[0]);

            Assert.True(items.Count() > 1);

            bool found = false;

            items.First().Tags.ToList().ForEach(tag =>
            {
                if (tag.Name.Contains(tags[0]))
                {
                    found = true;
                }
            });

            Assert.True(found);
        }

        [Fact]
        public async Task RetrieveTagsReturnsResultOnBigList()
        {
            List<PocketTag> items = (await client.GetTags()).ToList();

            items.ForEach(tag =>
            {
                Assert.True(tags.Contains(tag.Name));
            });

            Assert.Equal(items.Count, tags.Length);
        }

        private async Task FillAccount(int offset, int count)
        {
            int r;
            int r2;
            string[] tag;
            Random rnd = new Random();

            var items = urls.Skip(offset).Take(count)
                .Select((value, idx) => new { Value = value, Index = idx })
                .GroupBy(item => item.Index / 100, item => item.Value)
                .Cast<IEnumerable<string>>();

            foreach (IEnumerable<string> urlGroup in items)
            {
                await Task.WhenAll(urlGroup.Select(url =>
                {
                    r = rnd.Next(tags.Length);
                    r2 = rnd.Next(tags.Length);
                    tag = new string[] { tags[r], tags[r2] };
                    return client.Add(new Uri("http://" + url), tag);
                }));
            }
        }

        //[Fact]
        //public async Task Fillll()
        //{
        //  await FillAccount(11000, 89999);
        //}
    }
}