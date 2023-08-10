namespace B1NARY.Steamworks
{
	using B1NARY.Scripting;
	using global::Steamworks;
	using Steamworks;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.SocialPlatforms.Impl;
	using static B1NARY.Steamworks.SteamAchievement;

	public class AchievementManager : MonoBehaviour
	{
		public static CommandArray Commands = new()
		{
			["setachievement"] = (Action<string>)((key) =>
			{
				if (!SteamManager.HasInstanceOrInitialize())
					return;
				SteamAchievement target = FromKey(key);
				if (!target.Exists)
					Debug.Log($"{key} is not real");
				target.Unlock();
			}),
		};

		public void SetAchievement(string indexKey)
		{
			if (!SteamManager.HasInstanceOrInitialize())
				return;
			SteamAchievement target = AllAchievements.First(achievement => achievement.AchievementIndex == indexKey);
			target.Unlock();
		}
	}


	public record SteamAchievement(string AchievementIndex, string Name, string Description)
	{
		public static IReadOnlyList<SteamAchievement> AllAchievements { get; } = new SteamAchievement[]
		{
			new SteamAchievement("nut_cracker", "Nut Cracker", ""),
			new SteamAchievement("second_swallow", "Second Swallow", ""),
			new SteamAchievement("intern_UwU", "Intern", ""),
			new SteamAchievement("male_route", "I REALLY want a cup of Coffee...", ""),
			new SteamAchievement("female_route", "Jujitsu Training did NOT Pay off...", ""),
			new SteamAchievement("demo_female_hscene", "Yuri between the sheets.", ""),
			new SteamAchievement("demo_male_hscene", "Hentai Protag.", ""),
			new SteamAchievement("closed_beta_demo", "Beta Tester", ""),
			AllAchievementsCompleted,
		};
		private static SteamAchievement AllAchievementsCompleted = new("demo_complete", "Cultured", "");
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void CheckAllAchievements()
		{
			for (int i = 0; i < AllAchievements.Count; i++)
			{
				if (ReferenceEquals(AllAchievements[i], AllAchievementsCompleted))
					continue;
				if (!AllAchievements[i].Achieved)
					continue;
				return;
			}
			AllAchievementsCompleted.Unlock();
		}
		public static SteamAchievement FromKey(string achievementKey)
		{
			int index = Array.FindIndex(AllAchievements as SteamAchievement[],
				achievement => achievement.AchievementIndex.Contains(achievementKey));
			if (index == -1)
				return new SteamAchievement("null", "null", "unknown achievement") { m_exists = false, m_achieved = false };
			return AllAchievements[index];
		}

		public bool Exists
		{
			get
			{
				if (m_exists is null)
				{
					m_exists = SteamUserStats.GetAchievement(AchievementIndex, out bool outset);
					m_achieved = outset;
				}
				return m_exists.Value;
			}
		}
		private bool? m_exists = null;
		public bool Achieved 
		{
			get
			{
				if (m_achieved is null)
				{
					m_exists = SteamUserStats.GetAchievement(AchievementIndex, out bool outset);
					m_achieved = outset;
				}
				return m_achieved.Value;
			}
			private set => m_achieved = value; 
		}
		private bool? m_achieved = null;

		public void Unlock()
		{
			if (Achieved)
				return;
			if (!SteamManager.HasInstanceOrInitialize())
				return;
			Achieved = true;
			SteamUserStats.SetAchievement(AchievementIndex);
			SteamUserStats.StoreStats();
			CheckAllAchievements();
		}
	}
}
