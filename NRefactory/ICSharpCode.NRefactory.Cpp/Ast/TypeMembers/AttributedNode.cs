﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace ICSharpCode.NRefactory.Cpp
{
	public abstract class AttributedNode : AstNode
	{
		public static readonly Role<AttributeSection> AttributeRole = new Role<AttributeSection>("Attribute");
		public static readonly Role<AttributeSection> UnattachedAttributeRole = new Role<AttributeSection>("UnattachedAttribute");
		public static readonly Role<CppModifierToken> ModifierRole = new Role<CppModifierToken>("Modifier");
		
		public AstNodeCollection<AttributeSection> Attributes {
			get { return base.GetChildrenByRole (AttributeRole); }
		}
		
		public Modifiers Modifiers {
			get { return GetModifiers(this); }
			set { SetModifiers(this, value); }
		}

        public AstNodeCollection<CppModifierToken> ModifierTokens
        {
			get { return GetChildrenByRole (ModifierRole); }
		}
		
		internal static Modifiers GetModifiers(AstNode node)
		{
			Modifiers m = 0;
			foreach (CppModifierToken t in node.GetChildrenByRole (ModifierRole)) {
				m |= t.Modifier;
			}
			return m;
		}
		
		internal static void SetModifiers(AstNode node, Modifiers newValue)
		{
			Modifiers oldValue = GetModifiers(node);
			AstNode insertionPos = node.GetChildrenByRole(AttributeRole).LastOrDefault();
			foreach (Modifiers m in CppModifierToken.AllModifiers) {
				if ((m & newValue) != 0) {
					if ((m & oldValue) == 0) {
						// Modifier was added
						var newToken = new CppModifierToken(TextLocation.Empty, m);
						node.InsertChildAfter(insertionPos, newToken, ModifierRole);
						insertionPos = newToken;
					} else {
						// Modifier already exists
						insertionPos = node.GetChildrenByRole(ModifierRole).First(t => t.Modifier == m);
					}
				} else {
					if ((m & oldValue) != 0) {
						// Modifier was removed
						node.GetChildrenByRole (ModifierRole).First(t => t.Modifier == m).Remove();
					}
				}
			}
		}
		
		protected bool MatchAttributesAndModifiers (AttributedNode o, PatternMatching.Match match)
		{
			return (this.Modifiers == Modifiers.Any || this.Modifiers == o.Modifiers) && this.Attributes.DoMatch (o.Attributes, match);
		}
		
		public bool HasModifier (Modifiers mod)
		{
			return (Modifiers & mod) == mod;
		}
	}
}
