using System;
using System.Collections.Generic;
namespace YourNamespace
{
    public sealed class AutomatonMatch
    {
        public int StartIndex { get; }
        public int Length { get; }
        public string Value { get; }

        public AutomatonMatch(int startIndex, int length, string value)
        {
            StartIndex = startIndex;
            Length = length;
            Value = value;
        }
    }

    public sealed class AutomatonNode
    {
        public int Id { get; }
        public bool IsAccepting { get; }
        public Dictionary<string, int> Transitions { get; } = new Dictionary<string, int>();

        public AutomatonNode(int id, bool isAccepting = false)
        {
            Id = id;
            IsAccepting = isAccepting;
        }
    }

    public sealed class FiniteAutomaton
    {
        private readonly Dictionary<int, AutomatonNode> _nodes;

        public int StartNodeId { get; }

        private FiniteAutomaton(Dictionary<int, AutomatonNode> nodes, int startNodeId)
        {
            _nodes = nodes;
            StartNodeId = startNodeId;
        }

        public static FiniteAutomaton CreateEthereumAddressAutomaton()
        {
            var nodes = new Dictionary<int, AutomatonNode>();

            nodes[0] = new AutomatonNode(0);
            nodes[1] = new AutomatonNode(1);
            nodes[2] = new AutomatonNode(2);

            for (int i = 3; i <= 42; i++)
            {
                nodes[i] = new AutomatonNode(i, isAccepting: i == 42);
            }

            nodes[0].Transitions["0"] = 1;
            nodes[1].Transitions["x"] = 2;
            nodes[1].Transitions["X"] = 2;

            for (int i = 2; i <= 41; i++)
            {
                nodes[i].Transitions["HEX"] = i + 1;
            }

            return new FiniteAutomaton(nodes, startNodeId: 0);
        }

        public IReadOnlyList<AutomatonMatch> FindMatches(string text)
        {
            var matches = new List<AutomatonMatch>();

            for (int startIndex = 0; startIndex < text.Length; startIndex++)
            {
                int currentNodeId = StartNodeId;
                int acceptedEndExclusive = -1;

                for (int index = startIndex; index < text.Length; index++)
                {
                    string symbolClass = Classify(text[index]);
                    if (!TryMoveNext(currentNodeId, symbolClass, out currentNodeId))
                    {
                        break;
                    }

                    if (_nodes[currentNodeId].IsAccepting)
                    {
                        acceptedEndExclusive = index + 1;
                    }
                }

                if (acceptedEndExclusive != -1)
                {
                    int length = acceptedEndExclusive - startIndex;
                    string value = text.Substring(startIndex, length);
                    matches.Add(new AutomatonMatch(startIndex, length, value));
                }
            }

            return matches;
        }

        public string BuildGraphDescription()
        {
            var lines = new List<string>
            {
                "Ethereum automaton graph",
                "S0 --'0'--> S1",
                "S1 --'x'--> S2",
                "S2..S41 --HEX--> next state",
                "S42 -- accepting state",
                "HEX = 0-9 or a-f / A-F",
                "Recognized pattern: 0x + 40 hexadecimal symbols"
            };

            return string.Join(Environment.NewLine, lines);
        }

        private bool TryMoveNext(int currentNodeId, string symbolClass, out int nextNodeId)
        {
            if (_nodes[currentNodeId].Transitions.TryGetValue(symbolClass, out nextNodeId))
            {
                return true;
            }

            if (symbolClass == "0" && _nodes[currentNodeId].Transitions.TryGetValue("HEX", out nextNodeId))
            {
                return true;
            }

            return false;
        }

        private static string Classify(char symbol)
        {
            if (symbol == '0')
            {
                return "0";
            }

            if (symbol == 'x')
            {
                return "x";
            }

            if (symbol == 'X')
            {
                return "X";
            }

            if (Uri.IsHexDigit(symbol))
            {
                return "HEX";
            }

            return "OTHER";
        }
    }
}
