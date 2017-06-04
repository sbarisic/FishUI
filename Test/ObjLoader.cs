﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;

using Vector3 = Test.fgl_vec3;
using Vector2 = Test.fgl_vec2;
using Tri = Test.fgl_triangle;

namespace Test {
	static class ObjLoader {
		static void ParseFaceElement(string Element, out int VertInd, out int UVInd) {
			string[] ElementTokens = Element.Trim().Split('/');

			VertInd = int.Parse(ElementTokens[0]) - 1;

			UVInd = 0;
			if (ElementTokens[1].Length != 0)
				UVInd = int.Parse(ElementTokens[1]) - 1;
		}

		static void ParseFace(string[] Tokens, out int[] VertInds, out int[] UVInds) {
			VertInds = new int[Tokens.Length];
			UVInds = new int[Tokens.Length];

			for (int i = 0; i < VertInds.Length; i++)
				ParseFaceElement(Tokens[i], out VertInds[i], out UVInds[i]);
		}

		static float ParseFloat(string Str) {
			return float.Parse(Str, CultureInfo.InvariantCulture);
		}

		public static Tri[] Load(string[] Lines) {
			List<Vector3> Verts = new List<Vector3>();
			List<Vector2> UVs = new List<Vector2>();

			List<Tri> Tris = new List<Tri>();

			for (int i = 0; i < Lines.Length; i++) {
				string L = Lines[i].ToLower().Trim();

				while (L.Contains("  "))
					L = L.Replace("  ", " ");

				if (L.Length == 0 || L.StartsWith("#") || L == "\0")
					continue;

				string[] Tokens = L.Split(' ');


				switch (Tokens[0]) {
					case "v": {
							Verts.Add(new Vector3(ParseFloat(Tokens[1]), ParseFloat(Tokens[2]), ParseFloat(Tokens[3])));
							break;
						}

					case "vt": { // Texture coords
							UVs.Add(new Vector2(ParseFloat(Tokens[1]), ParseFloat(Tokens[2])));
							break;
						}

					case "vn": { // Vertex normals
							break;
						}

					case "f": { // Face
							int[] VertInds;
							int[] UVInds;

							ParseFace(Tokens.Skip(1).ToArray(), out VertInds, out UVInds);

							for (int j = 0; j < VertInds.Length; j++)
								if (VertInds[j] < 0) VertInds[j] = Verts.Count - VertInds[j];
							for (int j = 0; j < UVInds.Length; j++)
								if (UVInds[j] < 0) UVInds[j] = UVs.Count - UVInds[j];

							/*Tris.Add(Verts[VertInds[0] - 1]);
							Tris.Add(Verts[VertInds[1] - 1]);
							Tris.Add(Verts[VertInds[2] - 1]);*/

							if (VertInds.Length == 3) { // Triangles
								Tri T = new Tri();
								T.A = Verts[VertInds[0]];
								T.B = Verts[VertInds[1]];
								T.C = Verts[VertInds[2]];

								T.A_UV = UVs[UVInds[0]];
								T.B_UV = UVs[UVInds[1]];
								T.C_UV = UVs[UVInds[2]];
								Tris.Add(T);
							} else if (VertInds.Length == 4) { // Quads
								Tri T1 = new Tri();
								T1.A = Verts[VertInds[0]];
								T1.B = Verts[VertInds[1]];
								T1.C = Verts[VertInds[2]];

								T1.A_UV = UVs[UVInds[0]];
								T1.B_UV = UVs[UVInds[1]];
								T1.C_UV = UVs[UVInds[2]];
								Tris.Add(T1);

								Tri T2 = new Tri();
								T2.A = Verts[VertInds[2]];
								T2.B = Verts[VertInds[3]];
								T2.C = Verts[VertInds[0]];

								T2.A_UV = UVs[UVInds[2]];
								T2.B_UV = UVs[UVInds[3]];
								T2.C_UV = UVs[UVInds[0]];
								Tris.Add(T2);
							} else
								throw new NotImplementedException();
							break;
						}

					case "mtllib": {
							// TODO
							break;
						}

					case "usemtl": {
							break;
						}

					case "g": {
							break;
						}

					default:
						//Console.WriteLine("Unknown obj type: {0}", Tokens[0]);
						break;
				}
			}

			return Tris.ToArray();
		}

		public static Tri[] Load(string Path) {
			return Load(File.ReadAllLines(Path));
		}
	}
}
