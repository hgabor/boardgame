using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Level14.BoardGameRules.RegExp
{
    public class Pattern
    {
        IEnumerable<PatternElement> pattern;

        [Flags]
        public enum Direction {
            Up = 1,
            Down = 2,
            Left = 4,
            Right = 8,
            UpLeft = 16,
            UpRight = 32,
            DownLeft = 64,
            DownRight = 128,
            Any = Up | Down | Left | Right | UpLeft | UpRight | DownLeft | DownRight,
        }

        public Pattern(IEnumerable<PatternElement> pattern)
        {
            this.pattern = pattern;
        }

        private delegate Coords Incrementer(Coords c);
        private IEnumerable<Incrementer> ToIncrementers(Direction d)
        {
            List<Incrementer> list = new List<Incrementer>();
            if (d.HasFlag(Direction.Up)) list.Add(c => new Coords(c[0], c[1] - 1));
            if (d.HasFlag(Direction.Down)) list.Add(c => new Coords(c[0], c[1] + 1));
            if (d.HasFlag(Direction.Left)) list.Add(c => new Coords(c[0] - 1, c[1]));
            if (d.HasFlag(Direction.Right)) list.Add(c => new Coords(c[0] + 1, c[1]));
            if (d.HasFlag(Direction.UpLeft)) list.Add(c => new Coords(c[0] - 1, c[1] - 1));
            if (d.HasFlag(Direction.UpRight)) list.Add(c => new Coords(c[0] + 1, c[1] - 1));
            if (d.HasFlag(Direction.DownLeft)) list.Add(c => new Coords(c[0] - 1, c[1] + 1));
            if (d.HasFlag(Direction.DownRight)) list.Add(c => new Coords(c[0] + 1, c[1] + 1));
            return list;
        }

        private bool FindMatch(GameState state, Coords input, Coords lastInput, Incrementer inc, IEnumerable<PatternElement> restOfPattern, out List<Coords> output)
        {
            if (restOfPattern.Count() == 0)
            {
                // Nothing more to match, make sure the last matched input
                // was the actual end of the input:
                output = new List<Coords>();
                if (Coords.Match(inc(lastInput), input))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            var currentPattern = restOfPattern.First();
            var newRestOfPattern = restOfPattern.Skip(1);

            foreach (var count in currentPattern.Count)
            {
                // Try all combinations of the pattern count/range
                Coords currentInput = input;
                List<Coords> currentCapture = new List<Coords>();
                for (int i = 0; i < count; i++) {
                    // All elements must match
                    if (currentPattern.Predicate(state, input))
                    {
                        currentCapture.Add(currentInput);
                        currentInput = inc(currentInput);
                    }
                    else
                    {
                        goto tryDifferentCount;
                    }
                }
                // Current round matches, try the rest
                List<Coords> newCapture;
                if (FindMatch(state, currentInput, lastInput, inc, newRestOfPattern, out newCapture))
                {
                    currentCapture.AddRange(newCapture);
                    output = currentCapture;
                    return true;
                }
            tryDifferentCount: { }
            }
            output = null;
            return false;
        }

        public bool CaptureAll(GameState state, Coords start, Coords end, Direction dir, out IEnumerable<Coords> capture)
        {
            if (pattern.Count() == 0)
            {
                capture = new List<Coords>();
                return true;
            }
            if (!pattern.ElementAt(0).IsTarget)
            {
                throw new NotImplementedException("Target other than the beginning is not supported yet.");
            }
            List<Coords> cAll = new List<Coords>();
            bool retVal = false;
            foreach (var incrementer in ToIncrementers(dir))
            {
                // Try every direction
                List<Coords> c = new List<Coords>();
                if (FindMatch(state, start, end, incrementer, pattern, out c))
                {
                    cAll.AddRange(c);
                    retVal = true;
                }
            }
            capture = cAll;
            return retVal;
        }

        public bool Match(GameState state, Coords start, Coords end, Direction dir)
        {
            if (pattern.Count() == 0)
            {
                return true;
            }
            if (!pattern.ElementAt(0).IsTarget)
            {
                throw new NotImplementedException("Target other than the beginning is not supported yet.");
            }
            foreach (var incrementer in ToIncrementers(dir))
            {
                // Try every direction
                List<Coords> c;
                if (FindMatch(state, start, end, incrementer, pattern, out c))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
