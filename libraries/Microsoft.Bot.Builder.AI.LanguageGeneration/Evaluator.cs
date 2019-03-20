﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{

    class EvaluationTarget
    {
        public EvaluationTarget(string templateName, object scope)
        {
            TemplateName = templateName;
            Scope = scope;
        }

        public string TemplateName { get; set; }
        public object Scope { get; set; }
    }
    class Evaluator: LGFileParserBaseVisitor<string>
    { 
        public readonly EvaluationContext Context;

        private readonly IGetMethod GetMethodX;
        private readonly IGetValue GetValueX;

        
        private Stack<EvaluationTarget> evalutationTargetStack = new Stack<EvaluationTarget>();

        public Evaluator(EvaluationContext context, IGetMethod getMethod, IGetValue getValue)
        {
            Context = context;
            GetMethodX = getMethod ?? new GetMethodExtensions(this);
            GetValueX = getValue ?? new GetValueExtensions(this);
        }


        public string EvaluateTemplate(string templateName, object scope)
        {
            if (!Context.TemplateContexts.ContainsKey(templateName))
            {
                throw new Exception($"No such template: {templateName}");
            }

            if (evalutationTargetStack.Any(e => e.TemplateName == templateName))
            { 
                throw new Exception($"Loop detected: {String.Join(" => ", evalutationTargetStack.Reverse().Select(e => e.TemplateName))} => {templateName}");
            }

            // Using a stack to track the evalution trace
            evalutationTargetStack.Push(new EvaluationTarget(templateName, scope)); 
            string result = Visit(Context.TemplateContexts[templateName]);
            evalutationTargetStack.Pop();

            return result;
        }

        public override string VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var templateNameContext = context.templateNameLine();
            if (templateNameContext.templateName().GetText().Equals(CurrentTarget().TemplateName))
            {
                if(context.templateBody() == null)
                {
                    throw new Exception($"There is no template body in template {CurrentTarget().TemplateName}");
                }
                return Visit(context.templateBody());
            }
            return null;
        }

        public override string VisitNormalBody([NotNull] LGFileParser.NormalBodyContext context)
        {
            return Visit(context.normalTemplateBody());
        }

        public override string VisitNormalTemplateBody([NotNull] LGFileParser.NormalTemplateBodyContext context)
        {
            var normalTemplateStrs = context.normalTemplateString();
            Random rd = new Random();
            return Visit(normalTemplateStrs[rd.Next(normalTemplateStrs.Length)]);
        }

        public override string VisitConditionalBody([NotNull] LGFileParser.ConditionalBodyContext context)
        {
            var caseRules = context.conditionalTemplateBody().caseRule();
            foreach (var caseRule in caseRules)
            {
                var conditionExpression = caseRule.caseCondition().EXPRESSION().GetText();
                if (EvalCondition(conditionExpression))
                {
                    return Visit(caseRule.normalTemplateBody());
                }
            }

            if (context?.conditionalTemplateBody()?.defaultRule() != null)
            {
                return Visit(context.conditionalTemplateBody().defaultRule().normalTemplateBody());
            }
            else
            {
                return null;
            }
        }
        

        public override string VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            var builder = new StringBuilder();
            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.DASH:
                        break;
                    case LGFileParser.ESCAPE_CHARACTER:
                        builder.Append(EvalEscapeCharacter(node.GetText()));
                        break;
                    case LGFileParser.INVALID_ESCAPE:
                        throw new Exception($"escape character {node.GetText()} is invalid");
                    case LGFileParser.EXPRESSION:
                        builder.Append(EvalExpression(node.GetText()));
                        break;
                    case LGFileParser.TEMPLATE_REF:
                        builder.Append(EvalTemplateRef(node.GetText()));
                        break;
                    case LGFileLexer.MULTI_LINE_TEXT:
                        builder.Append(EvalMultiLineText(node.GetText()));
                        break;
                    default:
                        builder.Append(node.GetText());
                        break;
                }
            }
            return builder.ToString();
        }

        private string EvalEscapeCharacter(string exp)
        {
            var validCharactersDict = new Dictionary<string, string> {
                //Top four items :C# later render engine will treat them as escape characters, so the format is unchanged
                { @"\r","\r"},
                { @"\n","\n"},
                { @"\t","\t"},
                { @"\\","\\"},

                { @"\[","["},
                { @"\]","]"},
                { @"\{","{"},
                { @"\}","}"},
            };

            if (validCharactersDict.ContainsKey(exp))
                return validCharactersDict[exp];

            throw new Exception($"escape character {exp} is invalid");

        }

        private bool EvalCondition(string exp)
        {
            try
            {
                exp = exp.TrimStart('{').TrimEnd('}');
                var result = EvalByExpressionEngine(exp, CurrentTarget().Scope); 

                if ((result is Boolean r1 && r1 == false) ||
                    (result is int r2 && r2 == 0))
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Expression {exp} evaled as false due to exception");
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        private string EvalExpression(string exp)
        {
            exp = exp.TrimStart('{').TrimEnd('}');
            var result = EvalByExpressionEngine(exp, CurrentTarget().Scope);
            return result.ToString();
        }

        private string EvalTemplateRef(string exp)
        {
            exp = exp.TrimStart('[').TrimEnd(']').Trim();

            var argsStartPos = exp.IndexOf('(');
            if (argsStartPos > 0) // Do have args
            {
                // EvaluateTemplate all arguments using ExpressoinEngine
                var argsEndPos = exp.LastIndexOf(')');
                if (argsEndPos < 0 || argsEndPos < argsStartPos+1)
                {
                    throw new Exception($"Not a valid template ref: {exp}");
                }
                var argExpressions = exp.Substring(argsStartPos + 1, argsEndPos - argsStartPos - 1).Split(',');
                var args = argExpressions.Select(x => EvalByExpressionEngine(x, CurrentTarget().Scope)).ToList();

                // Construct a new Scope for this template reference
                // Bind all arguments to parameters
                var templateName = exp.Substring(0, argsStartPos);
                var newScope = ConstructScope(templateName, args);

                return EvaluateTemplate(templateName, newScope);
                
            }
            return EvaluateTemplate(exp, CurrentTarget().Scope);
        }

        private EvaluationTarget CurrentTarget()
        {
            // just don't want to write evaluationTargetStack.Peek() everywhere
            return evalutationTargetStack.Peek();
        }

        private string EvalMultiLineText(string exp)
        {
            exp = exp.Substring(3, exp.Length - 6); //remove ``` ```
            var reg = @"@\{[^{}]+\}";
            var evalutor = new MatchEvaluator(m =>
            {
                var newExp = m.Value.Substring(1); // remove @
                if (newExp.StartsWith("{[") && newExp.EndsWith("]}"))
                {
                    return EvalTemplateRef(newExp.Substring(2, newExp.Length - 4));//[ ]
                }
                else
                {
                    return EvalExpression(newExp).ToString();//{ }
                }
            });

            return Regex.Replace(exp, reg, evalutor);
        }
        private List<string> ExtractParameters(string templateName)
        {
            bool hasParameters = Context.TemplateParameters.TryGetValue(templateName, out List<string> parameters);
            return hasParameters ? parameters : new List<string>();
        }

        private object EvalByExpressionEngine(string exp, object scope)
        {
            return ExpressionEngine.Evaluate(exp, scope, GetValueX.GetValueX, GetMethodX.GetMethodX);
        }

        public object ConstructScope(string templateName, List<object> args)
        {
            if (args.Count == 1 && 
                !Context.TemplateParameters.ContainsKey(templateName))
            {
                // Special case, if no parameters defined, and only one arg, don't wrap
                return args[0];
            }

            var paramters = ExtractParameters(templateName);

            if (paramters.Count != args.Count)
            {
                throw new Exception($"Arguments count mismatch for template ref {templateName}, expected {paramters.Count}, actual {args.Count}");
            }

            var newScope = paramters.Zip(args, (k, v) => new { k, v })
                                    .ToDictionary(x => x.k, x => x.v);

            return newScope;
        }

    }
}
