using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;

/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2014 Samuel Jack
 * 
 * http://blog.functionalfun.net/2009/06/introduction-to-ui-automation-with.html
 * https://github.com/samueldjack/uiautomation-spirographs
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */

namespace CasterUIAutomation
{
    public static class AutomationExtensions
    {
        public static void EnsureElementIsScrolledIntoView(this AutomationElement element)
        {
            if (!element.Current.IsOffscreen)
            {
                return;
            }

            if (!(bool)element.GetCurrentPropertyValue(AutomationElement.IsScrollItemPatternAvailableProperty))
            {
                return;
            }

            var scrollItemPattern = element.GetScrollItemPattern();
            scrollItemPattern.ScrollIntoView();
        }

        public static AutomationElement FindDescendentByConditionPath(this AutomationElement element, IEnumerable<Condition> conditionPath)
        {
            if (!conditionPath.Any())
            {
                return element;
            }

            var result = conditionPath.Aggregate(
                element,
                (parentElement, nextCondition) => parentElement == null
                                                      ? null
                                                      : parentElement.FindChildByCondition(nextCondition));

            return result;
        }

        public static AutomationElement FindDescendentByIdPath(this AutomationElement element, IEnumerable<string> idPath)
        {
            var conditionPath = CreateConditionPathForPropertyValues(AutomationElement.AutomationIdProperty, idPath.Cast<object>());

            return FindDescendentByConditionPath(element, conditionPath);
        }

        public static AutomationElement FindDescendentByNamePath(this AutomationElement element, IEnumerable<string> namePath)
        {
            var conditionPath = CreateConditionPathForPropertyValues(AutomationElement.NameProperty, namePath.Cast<object>());

            return FindDescendentByConditionPath(element, conditionPath);
        }

        public static IEnumerable<Condition> CreateConditionPathForPropertyValues(AutomationProperty property, IEnumerable<object> values)
        {
            var conditions = values.Select(value => new PropertyCondition(property, value));

            return conditions.Cast<Condition>();
        }
        /// <summary>
        /// Finds the first child of the element that has a descendant matching the condition path.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="conditionPath">The condition path.</param>
        /// <returns></returns>
        public static AutomationElement FindFirstChildHavingDescendantWhere(this AutomationElement element, IEnumerable<Condition> conditionPath)
        {
            return
                element.FindFirstChildHavingDescendantWhere(
                    child => child.FindDescendentByConditionPath(conditionPath) != null);
        }

        /// <summary>
        /// Finds the first child of the element that has a descendant matching the condition path.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="conditionPath">The condition path.</param>
        /// <returns></returns>
        public static AutomationElement FindFirstChildHavingDescendantWhere(this AutomationElement element, Func<AutomationElement, bool> condition)
        {
            var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);

            foreach (AutomationElement child in children)
            {
                if (condition(child))
                {
                    return child;
                }
            }

            return null;
        }

        public static AutomationElement FindChildById(this AutomationElement element, string automationId)
        {
            var result = element.FindChildByCondition(
                new PropertyCondition(AutomationElement.AutomationIdProperty, automationId));

            return result;
        }

        public static AutomationElement FindChildByName(this AutomationElement element, string name)
        {
            var result = element.FindChildByCondition(
                new PropertyCondition(AutomationElement.NameProperty, name));

            return result;
        }

        public static AutomationElement FindChildByClass(this AutomationElement element, string className)
        {
            var result = element.FindChildByCondition(
                new PropertyCondition(AutomationElement.ClassNameProperty, className));

            return result;
        }

        public static AutomationElement FindChildByProcessId(this AutomationElement element, int processId)
        {
            var result = element.FindChildByCondition(
                new PropertyCondition(AutomationElement.ProcessIdProperty, processId));

            return result;
        }

        public static AutomationElement FindChildByControlType(this AutomationElement element, ControlType controlType)
        {
            var result = element.FindChildByCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, controlType));

            return result;
        }

        public static AutomationElement FindChildByCondition(this AutomationElement element, Condition condition)
        {
            var result = element.FindFirst(
                TreeScope.Children,
                condition);

            return result;
        }

        /// <summary>
        /// Finds the child text block of an element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static AutomationElement FindChildTextBlock(this AutomationElement element)
        {
            var child = TreeWalker.RawViewWalker.GetFirstChild(element);

            if (child != null && child.Current.ControlType == ControlType.Text)
            {
                return child;
            }

            return null;
        }
    }
}