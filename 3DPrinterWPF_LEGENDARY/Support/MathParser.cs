using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace _3DPrinterWPF_LEGENDARY
{
    /// <summary>
    /// Класс нужен для рабоыт с Математическими функциями, вводимыми пользователем
    /// </summary>
    public static class MathParser
    {
        private static readonly List<string> operators = new List<string>() { "+", "-", "*", "/", "^" };

        private static readonly List<string> functions = new List<string>()
        {   "sin",
            "cos",
            "tg",
            "ctg",

            "ln",
            "log",
            "log2",
           @"log[\d]",

            "sqrt",
            "cbrt",

            "arcsin",
            "arccos",
            "arctg",
            "arcctg",

            "sinh",
            "cosh",
            "tanh",
            "ctgh",

            "abs",
            "exp",

            "sec",
            "csc",

        };
        private static decimal ApplyFunction(decimal value, string function)
        {
            try
            {
                switch (function)
                {
                    case "sin":
                        return (decimal)Math.Sin((double)value);
                    case "cos":
                        return (decimal)Math.Cos((double)value);
                    case "tg":
                        return (decimal)Math.Tan((double)value);
                    case "ctg":
                        return 1 / (decimal)Math.Tan((double)value);

                    case "ln":
                        return (decimal)Math.Log((double)value);
                    case "log":
                        return (decimal)Math.Log10((double)value);
                    case "log2":
                        return (decimal)Math.Log2((double)value);

                    case "sqrt":
                        return (decimal)Math.Sqrt((double)value);
                    case "cbrt":
                        return (decimal)Math.Cbrt((double)value);
                    case @"log[\d]":
                        string pattern = @"\[(.*?)\]";
                        Match match = Regex.Match(function, pattern);
                        if (match.Success)
                        {
                            double result = Convert.ToDouble(match.Groups[1].Value);
                            return (decimal)(Math.Log((double)value) / Math.Log(result));
                        }
                        throw new PolishNotationException(function + "Тут ошибка где-то");

                    case "abs":
                        return (decimal)Math.Abs((double)value);
                    case "exp":
                        return (decimal)Math.Pow(Math.E, (double)value);

                    case "arcsin":
                        return (decimal)Math.Asin((double)value);
                    case "arccos":
                        return (decimal)Math.Acos((double)value);
                    case "arctg":
                        return (decimal)Math.Atan((double)value);
                    case "arcctg":
                        return (decimal)Math.PI / 2 - (decimal)Math.Atan((double)value);

                    case "sinh":
                        return (decimal)Math.Sinh((double)value);
                    case "cosh":
                        return (decimal)Math.Cosh((double)value);
                    case "tgh":
                        return (decimal)Math.Tanh((double)value);
                    case "ctgh":
                        return (decimal)(1 / Math.Tanh((double)value));

                    case "sec":
                        return (decimal)(1 / Math.Cos((double)value));
                    case "csc":
                        return (decimal)(1 / Math.Sin((double)value));


                    default:
                        throw new PolishNotationException("Invalid operator: " + value);
                }
            }
            catch
            {
                throw new OutOfScopeException();
            }
        }

        private static bool isNumber(string token) => double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double result);
        private static bool isFunction(string token) => functions.Contains(token);
        private static bool isOperator(string token) => operators.Contains(token);
        private static bool isVariable(string token)
        {
            if (isFunction(token)) return false;
            foreach (char c in token)
            {
                if (!char.IsLetter(c) || !((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
                {
                    return false;
                }
            }
            return true;
        }
        private static byte GetPriority(string @operator)
        {
            switch (@operator)
            {
                case "(":
                case ")":
                    return 0;
                case "+":
                case "-":
                    return 1;
                case "*":
                case "/":
                    return 2;
                case "^":
                    return 3;
                case "~":
                    return 4;
                default:
                    return 5;
            }
        }
        private static IEnumerable<string> Tokenization(string input)
        {
            string pattern = $@"(\d+([.,]\d+)?)|(({string.Join("|", functions)})(?=\())|([\+\-\*/\(\)^])|([a-zA-Z]{{1,4}})";

            MatchCollection matches = Regex.Matches(input, pattern);

            foreach (Match match in matches)
            {
                yield return match.Value;
            }
        }
        public static string[] TransformInfixToPostfixNotation(string infixString)
        {

            var stack = new Stack<string>();
            var outputQueue = new Queue<string>();

            string? prevToken = null;

            foreach (var token in Tokenization(infixString))
            {
                if (isNumber(token) || isVariable(token))
                {
                    outputQueue.Enqueue(token);
                }
                if (isFunction(token))
                {
                    stack.Push(token);
                }
                if (token == ",")
                {
                    while (stack.Peek() != "(")
                    {
                        outputQueue.Enqueue(stack.Pop());
                        if (stack.Count == 0) throw new PolishNotationException("Стек закончился до того, как был встречен токен открывающая скобка - в выражении пропущен разделитель аргументов функции (запятая), либо пропущена открывающая скобка.");
                    }

                }
                if (isOperator(token))
                {
                    string op = token;

                    if (op == "-" && (prevToken == null || operators.Contains(prevToken) || prevToken == "("))
                    {
                        op = "~";
                    }

                    while (stack.Count > 0 && GetPriority(op) <= GetPriority(stack.Peek()))
                    {
                        outputQueue.Enqueue(stack.Pop());
                    }
                    stack.Push(op);
                }
                if (token == "(")
                {
                    stack.Push(token);
                }
                else if (token == ")")
                {
                    while (stack.Count > 0 && stack.Peek() != "(")
                    {
                        outputQueue.Enqueue(stack.Pop());
                        if (stack.Count == 0) throw new PolishNotationException("стек закончился до того, как был встречен токен открывающая скобка. В выражении пропущена скобка");
                    }

                    stack.Pop();

                    if (stack.Count > 0 && isFunction(stack.Peek()))
                    {
                        outputQueue.Enqueue(stack.Pop());
                    }
                }
                if (token == "pi" || token == "PI" || token == "p" || token == "P")
                {
                    outputQueue.Enqueue("3,14159265359");
                }
                if (token == "e" || token == "E")
                {
                    outputQueue.Enqueue("2,71828182844");
                }
                prevToken = token;
            }

            while (stack.Count > 0)
            {
                if (stack.Peek() == "(") throw new PolishNotationException("Токен оператор на вершине стека — открывающая скобка, в выражении пропущена скобка");
                outputQueue.Enqueue(stack.Pop());
            }

            return outputQueue.ToArray();

        }

        //Служебные Функции Evaluate
        private static decimal ApplyOperator(decimal left, decimal right, string @operator)
        {
            try
            {
                switch (@operator)
                {
                    case "+":
                        return left + right;
                    case "-":
                        return left - right;
                    case "*":
                        return left * right;
                    case "/":
                        return left / right;
                    case "^":
                        if (left == right && left <= 0)
                        {
                            throw new OutOfScopeException();
                        }

                        return (decimal)Math.Pow((double)left, (double)right);

                    default:
                        throw new PolishNotationException("Invalid operator: " + @operator);
                }
            }
            catch
            {
                throw new OutOfScopeException();
            }

        }

        //Всё Ради Этого 
        public static decimal Evaluate(string function, decimal x)
        {
            Stack<decimal> stack = new Stack<decimal>();

            decimal right, left, value, result;

            foreach (var token in TransformInfixToPostfixNotation(function))
            {
                if (isNumber(token))
                {
                    stack.Push(Convert.ToDecimal(token));
                }

                else if (isVariable(token))
                {
                    stack.Push(x);
                }

                else if (isOperator(token))
                {
                    right = stack.Pop();
                    left = stack.Pop();
                    result = ApplyOperator(left, right, token);
                    stack.Push(result);
                }

                else if (token == "~")
                {
                    stack.Push(-1 * stack.Pop());
                }

                else if (isFunction(token))
                {
                    value = stack.Pop();
                    result = ApplyFunction(value, token);
                    stack.Push(result);
                }
            }

            return stack.Pop();
        }

        /// <summary>
        /// Версия для оптимизации, чтобы не разбивать функцию постоянно на польскую нотацию 
        /// </summary>
        /// <param name="functionInPolishNotation"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static decimal Evaluate(string[] functionInPolishNotation, decimal x)
        {
            Stack<decimal> stack = new Stack<decimal>();

            decimal right, left, value, result;

            foreach (var token in functionInPolishNotation)
            {
                if (isNumber(token))
                {
                    stack.Push(Convert.ToDecimal(token));
                }

                else if (isVariable(token))
                {
                    stack.Push(x);
                }

                else if (isOperator(token))
                {
                    right = stack.Pop();
                    left = stack.Pop();
                    result = ApplyOperator(left, right, token);
                    stack.Push(result);
                }

                else if (token == "~")
                {
                    stack.Push(-1 * stack.Pop());
                }

                else if (isFunction(token))
                {
                    value = stack.Pop();
                    result = ApplyFunction(value, token);
                    stack.Push(result);
                }
            }

            return stack.Pop();
        }

        
    }

    public class PolishNotationException : Exception
    {
        public PolishNotationException() : base() { }
        public PolishNotationException(string message) : base(message) { }
    }
    public class OutOfScopeException : Exception
    {
        public OutOfScopeException() : base() { }
        public OutOfScopeException(string message) : base(message) { }
    }
}
