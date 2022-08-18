using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1NARY.UI
{
	public class LoadingScreenAPI
	{
		public static readonly ReadOnlyCollection<string> loadingScreenTips = Array.AsReadOnly(new string[]
		{
			"If you want to eat something, chew it until its mushy and swallow.",
		});
		public static string PickNewRandomLoadingScreenTip(RandomFowarder.RandomType randomType)
			=> loadingScreenTips[RandomFowarder.Next(loadingScreenTips.Count, randomType)];

	}
}
