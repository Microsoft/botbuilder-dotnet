﻿//using Microsoft.Bot.Builder.Conversation;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static Microsoft.Bot.Builder.Conversation.RoutingRules;
//using static Microsoft.Bot.Builder.Conversation.Routers;

//namespace Microsoft.Bot.Builder.Conversation.Tests
//{
//    [TestClass]
//    [TestCategory("Routing - IfTrue")]
//    public class Routing_IfTrueTests
//    {
//        [TestMethod]
//        [ExpectedException(typeof(ArgumentNullException), "Expected ArgumentNullException")]
//        public async Task IfTrue_RequireParameters()
//        {
//            IfTrue(null, null);
//        }

//        [TestMethod]
//        [ExpectedException(typeof(ArgumentNullException), "Expected ArgumentNullException")]
//        public async Task IfTrue_RequireParametersAsync()
//        {
//            IfTrue(null, null);
//        }

//        [TestMethod]
//        public async Task IfTrue_ExecuteOnMatch()
//        {
//            bool result = false;
//            Router r = IfTrue(async (context) => new MatcherResult() { Result = true }, Simple(() => result = true));
//            Route route = await r.GetRoute(null);
//            await route.Action();
//            Assert.IsTrue(result == true, "Expected result to be TRUE");
//        }

//        [TestMethod]
//        public async Task IfTrue_NoMatchNoElseClause()
//        {
//            Router r = IfTrue((context) => false, Error());
//            Route route = await r.GetRoute(null);
//            Assert.IsNull(route, "Should be no route");
//        }

//        [TestMethod]
//        public async Task IfTrue_NoMatchRunsElseClauseNullRouter()
//        {
//            Router r = IfTrue((context) => false, Error(), Router.NoRouter());
//            Route route = await r.GetRoute(null);
//            Assert.IsNull(route, "Should be no route");
//        }

//        [TestMethod]
//        public async Task IfTrue_NoMatchRunsElseClause()
//        {
//            bool result = false;
//            Router r = IfTrue((context) => false, Error(), Simple(() => result = true));

//            Route route = await r.GetRoute(null);
//            await route.Action();

//            Assert.IsTrue(result == true, "Else clause did not run");
//        }

//        [TestMethod]
//        public async Task IfTrue_OnlyTheMainClauseRuns()
//        {
//            bool result = false;
//            Router r = IfTrue((context) => true, Simple(() => result = true), Error());
//            Route route = await r.GetRoute(null);
//            await route.Action();

//            Assert.IsTrue(result == true, "If clause did not run");
//        }
//    }
//}
