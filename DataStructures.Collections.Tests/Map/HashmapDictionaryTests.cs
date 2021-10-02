using DataStructures.Collections.Map;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DataStructures.Collections.Tests
{
    public class HashmapDictionaryTests
    {
        private IDictionary<int?, string> _map;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ZeroCapacityInitTest()
        {
            Assert.DoesNotThrow(() => new HashmapDictionary<int?, string>());
        }

        [Test]
        public void PredefinedZapacityInitTest()
        {
          Assert.DoesNotThrow(() => new HashmapDictionary<int?, string>(1564));
        }

        [Test]
        public void TestAddNull()
        {
            _map = new HashmapDictionary<int?, string>();
            Assert.Throws<ArgumentNullException>(() => _map.Add(null, string.Empty));
        }

        [Test]
        public void TestAddNegativeHashCodeKey()
        {
            _map = new HashmapDictionary<int?, string>();
            Assert.DoesNotThrow(() => _map.Add(-1231234, string.Empty));
        }

        [Test]
        public void TestAddWithSameKeyError()
        {
            _map = new HashmapDictionary<int?, string>();
            Assert.Throws<ArgumentException>(() => 
            {
                _map.Add(-1231234, string.Empty);
                _map.Add(-1231234, "test");
            });
        }

        [Test]
        public void TestAddNegativeHashCodeKeyWithBucketResize()
        {
            _map = new HashmapDictionary<int?, string>();
            for (int key = -1234; key < 0; key++)
            {
                _map.Add(key, key.ToString());
            }
        }

        [Test]
        public void TestRemove()
        {
            _map = new HashmapDictionary<int?, string>();
            _map.Add(1234, "test");
            Assert.IsTrue(_map.Remove(1234));
        }

        [Test]
        public void TestRemoveandAddToVacantPlace()
        {
            _map = new HashmapDictionary<int?, string>();
            _map.Add(1234, "test");
            _map.Add(12345, "test");
            _map.Add(123455, "test");
            Assert.IsTrue(_map.Remove(1234));
            _map.Add(12343, "12343");
        }
    }
}