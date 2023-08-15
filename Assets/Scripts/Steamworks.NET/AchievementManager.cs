#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif
namespace B1NARY.Steamworks
{
	using B1NARY.DataPersistence;
	using B1NARY.Scripting;
#if !DISABLESTEAMWORKS
	using global::Steamworks;
#endif
	using Steamworks;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.SocialPlatforms.Impl;
	using static B1NARY.Steamworks.Achievement;

	public class AchievementManager : MonoBehaviour
	{
		public static CommandArray Commands = new()
		{
			["setachievement"] = (Action<string>)((key) =>
			{
				Achievement target = FromKey(key);
				if (!target.Exists)
					Debug.Log($"{key} is not real");
				target.Unlock();
			}),
		};

		public void SetAchievement(string indexKey)
		{
			Achievement target = AllAchievements.First(achievement => achievement.AchievementIndex == indexKey);
			target.Unlock();
		}
	}


	public record Achievement(string AchievementIndex, string Name, string Description)
	{
		private static readonly Achievement
			MaleRoute = new("male_route", "I REALLY want a cup of Coffee...", "Male Route Complete"),
			FemaleRoute = new("female_route", "Jujitsu Training did NOT Pay off...", "Female Route Complete"),
			FemaleRouteHScene = new("demo_female_hscene", "Yuri between the sheets.", "Female H Scene First Time"),
			MaleRouteHScene = new("demo_male_hscene", "Hentai Protag.", "Male H Scene First Time"),
			AllAchievementsCompleted = new("demo_complete", "Cultured", "Demo Complete");

		public static IReadOnlyList<Achievement> AllAchievements { get; } = new Achievement[]
		{
			MaleRoute,
			FemaleRoute,
			FemaleRouteHScene,
			MaleRouteHScene,
			AllAchievementsCompleted,
			new Achievement("nut_cracker", "Nut Cracker", "Defend against Lucas"),
			new Achievement("second_swallow", "Second Swallow", "Make the Male Player say Swallow Twice in Game"),
			new Achievement("intern_UwU", "Intern", "FIrst New Game"),
			new Achievement("closed_beta_demo", "Beta Tester", "Performed Beta Testing for the Demo!"),
		};


		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void TryUnlockCompleteDemo()
		{
#if !DISABLESTEAMWORKS
			if (!SteamManager.HasInstanceOrInitialize())
				return;
#endif
			if (!MaleRoute.Achieved)
				return;
			if (!FemaleRoute.Achieved)
				return;
			if (!FemaleRouteHScene.Achieved)
				return;
			if (!MaleRouteHScene.Achieved)
				return;
			AllAchievementsCompleted.Unlock();
		}
		public static Achievement FromKey(string achievementKey)
		{
			for (int i = 0; i < AllAchievements.Count; i++)
			{
				if (!AllAchievements[i].AchievementIndex.Contains(achievementKey))
					continue;
				return AllAchievements[i];
			}
#if !DISABLESTEAMWORKS
			return new Achievement("null", "null", "unknown achievement") { m_exists = false, m_achieved = false };
#else
			return new Achievement("null", "null", "unknown achievement");
#endif
		}

		public bool Exists
		{
#if DISABLESTEAMWORKS
			get => true;
#else
			get
			{
				if (m_exists is null)
				{
					m_exists = SteamUserStats.GetAchievement(AchievementIndex, out bool outset);
					m_achieved = outset;
				}
				return m_exists.Value;
			}

#endif
		}
#if !DISABLESTEAMWORKS
		private bool? m_exists = null;
#endif
		public bool Achieved
		{
#if DISABLESTEAMWORKS
			get => PlayerConfig.Instance.savedAchievements.Contains(AchievementIndex);
			set
			{
				if (value)
					PlayerConfig.Instance.savedAchievements.Add(AchievementIndex);
				else
					PlayerConfig.Instance.savedAchievements.Remove(AchievementIndex);
			}
#else
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
#endif
		}
#if !DISABLESTEAMWORKS
		private bool? m_achieved = null;
#endif

		public void Unlock()
		{
			if (Achieved)
				return;
#if !DISABLESTEAMWORKS
			if (!SteamManager.HasInstanceOrInitialize())
				return;
#endif
			Achieved = true;
#if !DISABLESTEAMWORKS
			SteamUserStats.SetAchievement(AchievementIndex);
			SteamUserStats.StoreStats();
			TryUnlockCompleteDemo();
#endif
		}
	}
}
