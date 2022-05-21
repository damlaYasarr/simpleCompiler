using System;
using System.Collections.Generic;
using System.Linq;

namespace koffixlang
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
            Console.Write("koffix>");
            var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }
                var parser = new Parser(line);
                var syntaxTree = parser.Parse();

                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                PrettyPrint(syntaxTree.Root);
                Console.ForegroundColor = color;
                if(syntaxTree.Diagnostic.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    foreach (var diagnostic in syntaxTree.Diagnostic)
                    {
                        Console.WriteLine(diagnostic);
                    }
                    
                    Console.ForegroundColor = color;
                }
                else
                {   
                    var e = new Evaluator(syntaxTree.Root);
                    var result = e.Evaluate();
                    Console.WriteLine($"RESULTS:--->>{result}");
                }

                var lexer = new Lexer(line);
                while (true)
                {
                    var token = lexer.nexttoken();
                    if (token.Kind == SyntaxKind.EndOfToken)
                        break;
                    Console.WriteLine($"{token.Kind}: {token.Text}");
                    if (token.Value != null)
                    {
                        Console.WriteLine($"{token.Value}");
                    }
                    Console.WriteLine();

                }
            }
            

        }
       
    static void PrettyPrint(SyntaxNode node, string indent = "")
        {
            Console.Write(indent);
            Console.Write(node.Kind);
            if (node is SyntaxToken t && t.Value != null)
            {
                Console.Write("");
                Console.Write(t.Value);
            }
            Console.WriteLine();
            indent += "    ";
            foreach (var child in node.GetChildren())
            {
                PrettyPrint(child, indent);
            }
           
        }
    }
    enum SyntaxKind
    {
        NumberToken, WhiteSpaceToken, PlusToken,
        MinusToken, StarToken, SlashToken,
        OpenParanthesisToken, CloseParanthesisToken,
        BadToken, EndOfToken,
        BinaryExpressionSyntax,
        NumberExpressionSyntax, Equality,
        NameExpression,
        AssignmentExpressionSyntax,
        EqualsEqualsToken
    }
    class SyntaxToken :SyntaxNode 
    {
       public SyntaxToken(SyntaxKind kind, int position, string text, object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;

        }
        public override SyntaxKind Kind { get; }
        public int Position { get; }
        public string Text { get; }
        public object Value { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }
    }
    class Lexer
    {
        private readonly string _text;
        private int _position;
        private List<string> _diagnostics = new List<string>(); 
       public Lexer(string text)
        {
            _text = text;
        }
        public IEnumerable<string> Diagnostics => _diagnostics; 
        private char Current
        {
            get
            {
                if (_position >= _text.Length)
                    return '\0';
                return _text[_position];
            }
        }
        private void Next()
        {
            _position++;
        }
        public SyntaxToken nexttoken()
        {
            if (_position >= _text.Length)
            {
                return new SyntaxToken(SyntaxKind.EndOfToken, _position, "\0",null);
            }
            if (char.IsDigit(Current)){
                var start = _position;
                while (char.IsDigit(Current))
                    Next();
                
                var length = _position - start;
                var text = _text.Substring(start, length);
               if(!int.TryParse(text, out var value))
                {
                    _diagnostics.Add($"{_text} this input cannot be represented");
                }

                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
            }


            if (char.IsWhiteSpace(Current))
            {
                var start = _position;
                while (char.IsWhiteSpace(Current))
                    Next();

                var length = _position - start;
                var text = _text.Substring(start, length);
               
                return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text, null);
            }
            if (Current == '+')
            {
                return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);
            }
           else if (Current == '-')
            {
                return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null);
            }
           else if (Current == '*')
            {
                return new SyntaxToken(SyntaxKind.StarToken, _position++, "*", null);
            }
           else if (Current == '/')
            {
                return new SyntaxToken(SyntaxKind.SlashToken, _position++, "/", null);
            }
           else if (Current == '(')
            {
                return new SyntaxToken(SyntaxKind.OpenParanthesisToken, _position++, "(", null);
            }
            else if (Current == '=')
            {
                return new SyntaxToken(SyntaxKind.Equality, _position++, "=", null);
            }
            else if (Current == ')')
            {
                return new SyntaxToken(SyntaxKind.CloseParanthesisToken, _position++, ")", null);
            }

            _diagnostics.Add($"error bad character input :{Current} "); 
           return  new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position-1,1), null);
            
           
        }

    }
    abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }
        public abstract System.Collections.Generic.IEnumerable<SyntaxNode> GetChildren(); 


    }
    abstract class ExpressionSyntax: SyntaxNode
    {

    }
    sealed class NumberExpressionSyntax :  ExpressionSyntax
    {
        public NumberExpressionSyntax(SyntaxToken numberToken)
        {
            NumberToken = numberToken;
        }
        public override SyntaxKind Kind => SyntaxKind.NumberExpressionSyntax;
        public SyntaxToken NumberToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NumberToken;
        }
    }
    sealed class BinaryExpressionSyntax: ExpressionSyntax
    {
        public BinaryExpressionSyntax( ExpressionSyntax left, SyntaxNode operatorToken, ExpressionSyntax right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;


        }
        public ExpressionSyntax Left { get; }
        public SyntaxNode OperatorToken { get; }
        public ExpressionSyntax Right { get; }
        public override SyntaxKind Kind => SyntaxKind.BinaryExpressionSyntax;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right; 
        }
    }
    sealed class SyntaxTree
    {
        public SyntaxTree(IEnumerable<string> diagnostic, ExpressionSyntax root, SyntaxToken endOfFileToken)
        {
            Diagnostic = diagnostic.ToArray();
        
            Root = root;
            EndOfFileToken = endOfFileToken;
        }
        public IReadOnlyList<string> Diagnostic { get; }
        public ExpressionSyntax Root { get;  }
        public SyntaxToken EndOfFileToken { get; }



    }
    class Parser
    {
        private readonly SyntaxToken[] _tokens;
        private List<string> _diagnostics= new List<string>();
        private int _position;
        public Parser(String text)
        {
            
            var tokens = new System.Collections.Generic.List<SyntaxToken>();
            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.nexttoken();
                if(token.Kind!=SyntaxKind.WhiteSpaceToken && token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }

            } while (token.Kind != SyntaxKind.EndOfToken);
            _tokens = tokens.ToArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }
        public IEnumerable<string> Diagnostic => _diagnostics;
        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset; 
            if (index >= _tokens.Length)
            {
                return _tokens[_tokens.Length - 1];
            }
            return _tokens[index];
        }
        private SyntaxToken Current => Peek(0);
        private SyntaxToken nexttoken()
        {
            var current = Current;
            _position++;
            return current;
        }
        private SyntaxToken Match( SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return nexttoken();
            _diagnostics.Add($"error: <{Current.Kind}>, expected <{kind}>");
            return new SyntaxToken(kind, Current.Position, null, null); 
        }

        public SyntaxTree Parse()
        {
            var expression = ParseExpression();
             var endofffiletoken= Match(SyntaxKind.EndOfToken);
            return new SyntaxTree(_diagnostics, expression, endofffiletoken);
        }
        private ExpressionSyntax parseAssignment2()
        {
            return parseAssignmentExpression();
        }
        private ExpressionSyntax parseAssignmentExpression()
        {   if (Peek(0).Kind==SyntaxKind.Equality && Peek(1).Kind == SyntaxKind.EqualsEqualsToken)
            {
                var identifierToken = nexttoken();
                var operatorToken = nexttoken();
                var right = parseAssignmentExpression();
               
                return new AssignmentExpressionSyntax(identifierToken, operatorToken,right );
            }
            return ParseExpression();
        }

        public ExpressionSyntax ParseExpression()
        {
            var left = ParserPrimaryExpression();
            while(Current.Kind== SyntaxKind.PlusToken || Current.Kind == SyntaxKind.MinusToken
                || Current.Kind == SyntaxKind.StarToken || Current.Kind == SyntaxKind.SlashToken)
            {
                var operatorToken = nexttoken();
                var right = ParserPrimaryExpression();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
                 
            }
            while (Current.Kind == SyntaxKind.Equality)
            {
                var identifierToken = nexttoken();
         
               return new NameExpressionSyntax(identifierToken);
            }
            return left;
        }
        private ExpressionSyntax ParserPrimaryExpression()
        {
            var numberToken = Match(SyntaxKind.NumberToken);
            return new NumberExpressionSyntax(numberToken);
        }
    }
    class Evaluator
    {
        private readonly ExpressionSyntax _root;
        private readonly Dictionary<string, object> _variables;
        //, Dictionary<string, object> variables
        public Evaluator(ExpressionSyntax root)
        {
            this._root = root;
            
        }
        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(ExpressionSyntax node)
        {
            //binaryexpression
            //numberexrepssion

            if (node is NumberExpressionSyntax n)
                return (int) n.NumberToken.Value;
            if (node is BinaryExpressionSyntax b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);
                if (b.OperatorToken.Kind == SyntaxKind.PlusToken)
                    return left + right;
                else if (b.OperatorToken.Kind == SyntaxKind.MinusToken)
                    return left - right;
                else if (b.OperatorToken.Kind == SyntaxKind.StarToken)
                    return left * right;
                else if (b.OperatorToken.Kind == SyntaxKind.SlashToken)
                    return left / right;
                else if (b.OperatorToken.Kind == SyntaxKind.Equality)
                {
                    _variables.Add(left.ToString(), right);
                    return left = right;
                }
                    
                else
                
                     throw new Exception($"UNEXPRECTED BINATY OPERATOR{b.OperatorToken.Kind}");
                
           
            }
            throw new Exception($"UNEXPRECTED NODE{node.Kind}");
        }
    }

    sealed class NameExpressionSyntax : ExpressionSyntax
    {
         public NameExpressionSyntax(SyntaxToken identifierToken)
        {
            IdentifierToken = identifierToken;
        }
        public SyntaxToken IdentifierToken { get; }
        public override SyntaxKind Kind => SyntaxKind.NameExpression;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IdentifierToken;
        }
    }
    sealed class AssignmentExpressionSyntax : ExpressionSyntax
    {
        public AssignmentExpressionSyntax(SyntaxToken identifierToken, SyntaxToken equalsToken, ExpressionSyntax expression)
        {
            IdentifierToken = identifierToken;
            EqualsToken = equalsToken;
            Expression = expression;
        }
        public SyntaxToken IdentifierToken { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Expression { get; }
        public override SyntaxKind Kind => SyntaxKind.AssignmentExpressionSyntax;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IdentifierToken;
            yield return EqualsToken;
            yield return Expression;
        }
    }
}
