﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using System.ComponentModel.Composition;
using dnSpy.Contracts.Text;
using dnSpy.Shared.Themes;

namespace dnSpy.Text {
	[Export(typeof(ITextSnapshotColorizerProvider))]
	sealed class CachedColorsListColorizerProvider : ITextSnapshotColorizerProvider {
		public IEnumerable<ITextSnapshotColorizer> Create(ITextBuffer textBuffer) {
			CachedColorsListColorizer colorizer;
			if (textBuffer.Properties.TryGetProperty(typeof(CachedColorsListColorizer), out colorizer))
				yield return colorizer;
		}

		public static void AddColorizer(ITextBuffer textBuffer, CachedColorsList cachedColorsList, double priority) {
			if (textBuffer == null)
				throw new System.ArgumentNullException(nameof(textBuffer));
			if (cachedColorsList == null)
				throw new System.ArgumentNullException(nameof(cachedColorsList));
			textBuffer.Properties.GetOrCreateSingletonProperty(typeof(CachedColorsListColorizer), () => CachedColorsListColorizer.Create(cachedColorsList, priority));
		}
	}

	sealed class CachedColorsListColorizer : ITextSnapshotColorizer {
		readonly CachedColorsList cachedColorsList;
		readonly double priority;

		CachedColorsListColorizer(CachedColorsList cachedColorsList, double priority) {
			this.cachedColorsList = cachedColorsList;
			this.priority = priority;
		}

		internal static CachedColorsListColorizer Create(CachedColorsList cachedColorsList, double priority) =>
			new CachedColorsListColorizer(cachedColorsList, priority);

		public IEnumerable<ColorSpan> GetColorSpans(SnapshotSpan snapshotSpan) {
			int offs = snapshotSpan.Span.Start;
			int end = snapshotSpan.Span.End;

			var infoPart = cachedColorsList.Find(offs);
			while (offs < end) {
				int defaultTextLength, tokenLength;
				object color;
				if (!infoPart.FindByDocOffset(offs, out defaultTextLength, out color, out tokenLength))
					yield break;

				if (tokenLength != 0)
					yield return new ColorSpan(new Span(offs + defaultTextLength, tokenLength), ThemeUtils.GetColor(color), priority);

				offs += defaultTextLength + tokenLength;
			}
		}
	}
}
