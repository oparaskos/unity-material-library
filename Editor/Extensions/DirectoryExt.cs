using System.IO;

namespace HestiaMaterialImporter
{
	static class DirectoryExt
	{
		public static DirectoryInfo MakeParents(this string path)
		{
			return MakeParents(Directory.GetParent(path));
		}

		public static DirectoryInfo MakeParents(this DirectoryInfo dir)
		{
			DirectoryInfo parent = dir.Parent;
			if (parent == dir) throw new DirectoryNotFoundException();
			if (!parent.Exists) MakeParents(parent);
			dir.Create();
			return dir;
		}
	}
}
