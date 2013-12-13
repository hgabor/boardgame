using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Level14.BoardGameRules;
using Level14.BoardGameRules.RegExp;

namespace UnitTests
{
    [TestClass]
    public class PatternMatchingTests
    {
        private bool TargetPredicate(GameState state, Coords coords)
        {
            return Coords.Match(coords, new Coords(5, 7));
        }
        private bool TruePredicate(GameState state, Coords coords)
        {
            return true;
        }
        private bool TenPredicate(GameState state, Coords coords)
        {
            return coords[1] == 10;
        }

        [TestMethod]
        public void TestEmpty()
        {
            Pattern pattern = new Pattern(new PatternElement[] { });
            Assert.IsTrue(pattern.Match(null, new Coords(1, 2), Pattern.Direction.Any));
        }

        [TestMethod]
        public void TestTarget()
        {
            Pattern pattern = new Pattern(new PatternElement[] {
                new PatternElement { Count = new[] {1}, Predicate = TargetPredicate, IsTarget = true }
            });
            Assert.IsTrue(pattern.Match(null, new Coords(5, 7), Pattern.Direction.Any));
            Assert.IsFalse(pattern.Match(null, new Coords(7, 5), Pattern.Direction.Any));
        }

        [TestMethod]
        public void TestRange()
        {
            Pattern pattern = new Pattern(new PatternElement[] {
                new PatternElement { Count = new[] {1}, Predicate = TargetPredicate, IsTarget = true },
                new PatternElement { Count = new[] {1, 2, 3, 4, 5, 6}, Predicate = TruePredicate },
                new PatternElement { Count = new[] {1}, Predicate = TenPredicate },
            });
            Assert.IsFalse(pattern.Match(null, new Coords(5,7), Pattern.Direction.Up));
            Assert.IsTrue(pattern.Match(null, new Coords(5, 7), Pattern.Direction.Down));
        }
    }
}
