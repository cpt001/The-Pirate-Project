using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.Profiling;

using Den.Tools;
using Den.Tools.Matrices;
using Den.Tools.GUI;
using MapMagic.Core;
using MapMagic.Products;
using MapMagic.Nodes.GUI;
using MapMagic.Nodes.SplinesGenerators;

namespace MapMagic.Nodes.GUI
{
	public class UnitySplinesEditors
	{
		[UnityEditor.InitializeOnLoadMethod]
		static void EnlistInMenu ()
		{
			CreateRightClick.generatorTypes.Add(typeof(UnitySplinesOutput214));
		}
	}
}