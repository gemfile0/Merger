﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vectrosity;

namespace com.Gemfile.Merger
{
	public interface INavigationView: IBaseView
	{
		List<NavigationColorInfo> Set(
			List<NavigationInfo> navigationInfos, 
			Dictionary<int, ICardView> fields, 
			Bounds cardBounds
		);
		void Show(Vector2 directionFirst, Vector2 touchDelta);
		void Clear();
		void Hide();
	}
	
	class VectorLineInfo
	{
		internal VectorLine value;
		internal Vector2 sourcePosition;
		internal Vector2 targetPosition;
		internal Vector2 fullDelta;
	}

	public class NavigationColorInfo
	{
		internal int index;
		internal Color32 color;
	}
	
	public class NavigationView: BaseView, INavigationView
	{
		[SerializeField]
		Texture2D lineTexture;
		[SerializeField]
		Texture2D frontTexture;
		
		List<VectorLineInfo> vectorLineInfos;

		List<Color32> lineColors;

		public NavigationView()
		{
			vectorLineInfos = new List<VectorLineInfo>();
			lineColors = new List<Color32>() {
				Color.red,
				Color.white,
				Color.blue,
				Color.cyan,
				Color.green,
				Color.magenta,
				Color.yellow,
			}; 
		}
		
		public override void Init()
		{
			VectorLine.SetEndCap("Arrow", EndCap.Front, 0, lineTexture, frontTexture);
		}

		public List<NavigationColorInfo> Set(
			List<NavigationInfo> navigationInfos, 
			Dictionary<int, ICardView> fields, 
			Bounds cardBounds) 
		{
			Clear();
			
			var navigationColorInfos = new List<NavigationColorInfo>();
			navigationInfos.ForEach(navigationInfo => {
				var sourceIndex = navigationInfo.sourceIndex;
				var wheresCanMerge = navigationInfo.wheresCanMerge;
				var colorPicked = lineColors[UnityEngine.Random.Range(0, lineColors.Count)];
				
				wheresCanMerge.ForEach(whereCanMerge => {
					var sourcePosition = fields[sourceIndex].Transform.position;
					var targetPosition = fields[whereCanMerge.index].Transform.position;
					var delta = targetPosition - sourcePosition;
					var cardSize = new Vector3(
						delta.normalized.x * cardBounds.extents.x, 
						delta.normalized.y * cardBounds.extents.y
					);
					sourcePosition += cardSize/2;
					targetPosition -= cardSize/2;

					var linePoints = new List<Vector2>() { 
						Camera.main.WorldToScreenPoint(targetPosition), 
						Camera.main.WorldToScreenPoint(sourcePosition)
					};
					var vectorLine = new VectorLine("Line", linePoints, 90.0f, LineType.Continuous);
					vectorLine.rectTransform.SetParent(transform);
					vectorLine.endCap = "Arrow";
					vectorLine.smoothColor = true;
					vectorLine.SetColors (new List<Color32>{
						colorPicked
					});

					vectorLineInfos.Add(new VectorLineInfo {
						value = vectorLine,
						sourcePosition = linePoints[1],
						targetPosition = linePoints[0],
						fullDelta = linePoints[0] - linePoints[1]
					});
				});

				if (wheresCanMerge.Count > 0)
				{
					navigationColorInfos.Add(new NavigationColorInfo() {
						index = sourceIndex,
						color = colorPicked
					});
				}
			});

			return navigationColorInfos;
		}

		public void Show(Vector2 directionFirst, Vector2 touchDelta)
		{
			vectorLineInfos.ForEach(touchLineInfo => {
				var fullDelta = touchLineInfo.fullDelta;
				if (directionFirst == fullDelta.normalized)
				{
					var vectorLine = touchLineInfo.value;
					var sourcePosition = touchLineInfo.sourcePosition;

					var clampedDelta = Vector2.ClampMagnitude(touchDelta, fullDelta.magnitude);
					var clapmpedTargetPosition = sourcePosition + new Vector2(
						clampedDelta.x * Math.Abs(directionFirst.x), 
						clampedDelta.y * Math.Abs(directionFirst.y)
					);
					vectorLine.points2[0] = clapmpedTargetPosition;
					vectorLine.active = true;
					vectorLine.Draw();
				}
			});
		}

		public void Clear()
		{
			Hide();
			VectorLine.Destroy(vectorLineInfos.Select(info => info.value).ToList());
			vectorLineInfos.Clear();
		}

		public void Hide()
		{
			vectorLineInfos.ForEach(vectorLineInfo => {
				vectorLineInfo.value.active = false;
			});
		}

		public override void Reset()
		{
			Clear();
		}
	}
}
