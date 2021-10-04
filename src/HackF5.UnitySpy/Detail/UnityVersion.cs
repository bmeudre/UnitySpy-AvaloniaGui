namespace HackF5.UnitySpy.Detail 
{
    public struct UnityVersion 
    {
        public int Year;

        public int VersionWithinYear;

        public int SubversionWithinYear;

        public UnityVersion(int year, int versionWithinYear, int subversionWithinYear)
        {
            this.Year = year;
            this.VersionWithinYear = versionWithinYear;
            this.SubversionWithinYear = subversionWithinYear;
        }

        public override bool Equals(object obj) 
        {
            if (obj is UnityVersion other)
            {
                return other.Year == Year && 
                        other.VersionWithinYear == VersionWithinYear &&
                        other.SubversionWithinYear == SubversionWithinYear;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 27 + Year.GetHashCode();
            hash = hash * 23 + VersionWithinYear.GetHashCode();
            hash = hash * 13 + SubversionWithinYear.GetHashCode();
            return hash;
        }

        public override string ToString() 
        {
            return Year + "." + VersionWithinYear + "." + SubversionWithinYear;
        }
        
        public static UnityVersion Parse(string version)
        {
            string[] versionSplit = version.Split('.');
            return new UnityVersion(version[0], version[1], version[2]);
        }
        
        public static bool operator ==(UnityVersion a, UnityVersion b) {
			return a.Equals(b);
		}

		public static bool operator !=(UnityVersion a, UnityVersion b){
			return !(a == b);
		}
    }
}