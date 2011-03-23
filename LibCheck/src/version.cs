/*============================================================
**
** File:    Version.cool
**
** Author:  
**
** Purpose: 
**
** Date:    June 4, 1999
**
** Copyright (c) Microsoft, 1999
**
===========================================================*/
namespace System {

    using CultureInfo = System.Globalization.CultureInfo;

    // A Version object contains four hierarchical numeric components: major, minor,
    // revision and build.  Revision and build may be unspecified, which is represented 
    // internally as a -1.  By definition, an unspecified component matches anything 
    // (both unspecified and specified), and an unspecified component is "less than" any
    // specified component.

    /// <summary>
    ///    <para>
    ///       Represents the version number for a Common Language Runtime assembly.
    ///    </para>
    /// </summary>
    /// <remarks>
    ///    <para>
    ///       Version numbers consist of four parts: major, minor,
    ///       build, and revision. By default, only major and minor are used for binding to
    ///       an assembly.
    ///    </para>
    ///    <para>
    ///       Subsequent versions of an assembly that differ only by build or
    ///       revision numbers are considered to be Quick Fix Engineering (QFE) updates of
    ///       the prior version. If desired, build and revision can be honored by changing the version policy in the
    ///       configuration.
    ///    </para>
    /// </remarks>
     public sealed class MyVersion : ICloneable, IComparable
    {
        // AssemblyName depends on the order staying the same
        private int _Major;
        private int _Minor;
        private int _Revision = -1;
        private int _Build= -1;
    
        /// <overload>
        ///    <para>Overloaded. Initializes a new instance of the Version class with the specified major, minor, build, and revision numbers.</para>
        /// </overload>
        /// <summary>
        ///    <para>Initializes a new Version given the major version, minor
        ///       version, revision, and build numbers.</para>
        /// </summary>
        /// <param name='major'>The major version number.</param>
        /// <param name='minor'>The minor version number.</param>
        /// <param name='revision'>The revision number.</param>
        /// <param name=' build'>The build number.</param>
        /// <exception cref='ArgumentOutOfRangeException'>if <paramref name="major"/>, <paramref name="minor"/> , <paramref name="revision"/> or <paramref name="build"/> is less than 0. </exception>
        /// <remarks>
        ///    Metadata restricts the <paramref name="major"/>,
        /// <paramref name="minor"/>, <paramref name="revision"/>, and <paramref name="build"/> to a maximum value of 
        ///    UInt16.MaxValue-1.
        /// NOTE: The order of the third and fourth parameters (revision and build) will be reversed in Beta 2.
        /// </remarks>
        public MyVersion(int major, int minor, int revision, int build) {
            
            
            _Major = major;
            _Minor = minor;
            _Revision = revision;
            _Build = build;
        }
		 public MyVersion(Version v) {
            
            
            _Major = v.Major;
            _Minor = v.Minor;
            _Revision = v.Revision;
            _Build = v.Build;
        }

        /// <summary>
        ///    <para>Initializes a new Version given the major version, minor version, and revision.</para>
        /// </summary>
        /// <param name='major'>The major version number.</param>
        /// <param name='minor'>The minor version number.</param>
        /// <param name='revision'>The revision number.</param>
        /// <exception cref='ArgumentOutOfRangeException'>if <paramref name="major"/>, <paramref name="minor"/>, or <paramref name="revision"/> is less than 0. </exception>
        /// <remarks>
        /// <para>Metadata restricts the <paramref name="major"/>, <paramref name="minor"/>, <paramref name="revision"/>, and <paramref name="build"/> to a maximum value of UInt16.MaxValue-1.</para>
        /// </remarks>
        public MyVersion(int major, int minor, int revision) {
              
            
            _Major = major;
            _Minor = minor;
            _Revision = revision;
        }
    
        /// <summary>
        ///    <para>Initializes a new Version given the major version and minor version.</para>
        /// </summary>
        /// <param name='major'>The major version number.</param>
        /// <param name='minor'>The minor version number.</param>
        /// <exception cref='ArgumentOutOfRangeException'>if <paramref name="major"/> or <paramref name="minor"/> is less than 0. </exception>
        /// <remarks>
        /// <para>Metadata restricts the <paramref name="major"/>, <paramref name="minor"/>, <paramref name="revision"/>, and <paramref name="build"/> to a maximum value of UInt16.MaxValue-1.</para>
        /// </remarks>
        public MyVersion(int major, int minor) {

            _Major = major;
            _Minor = minor;
        }

        /// <summary>
        ///    <para>Initializes a new Version given
        ///       a version string in the format returned by <see cref='System.Version.ToString'/>.</para>
        /// </summary>
        /// <param name='version'>A version string.</param>
        /// <exception cref='ArgumentException'>if version has fewer than 2 components or more than 4 components. The only components recognized are major, minor, build, and version of which the major and minor components are mandatory.</exception>
        /// <exception cref='ArgumentNullException'>if <paramref name="version"/> is null</exception>
        /// <exception cref='ArgumentOutOfRangeException'>if <paramref name="major"/>, <paramref name="minor"/> , <paramref name="revision"/> or <paramref name="build"/> is less than 0.</exception>
        /// <remarks>
        /// <para>Metadata restricts the <paramref name="major"/>, <paramref name="minor"/>, <paramref name="revision"/>, and <paramref name="build"/> to a maximum value of UInt16.MaxValue-1.</para>
        /// </remarks>
        public MyVersion(String version) {
            if (version == null)
                throw new ArgumentNullException("version");

            String[] parsedComponents = version.Split(new char[] {'.'});
            int parsedComponentsLength = parsedComponents.Length;
            
            _Major = Int32.Parse(parsedComponents[0], CultureInfo.InvariantCulture);
            

            _Minor = Int32.Parse(parsedComponents[1], CultureInfo.InvariantCulture);
            
            parsedComponentsLength -= 2;
            if (parsedComponentsLength > 0) {
                _Revision = Int32.Parse(parsedComponents[2], CultureInfo.InvariantCulture);
            
                parsedComponentsLength--;
                if (parsedComponentsLength > 0) {
                    _Build = Int32.Parse(parsedComponents[3], CultureInfo.InvariantCulture);
                }
            }
        }

        public MyVersion() {
        }

        // Properties for setting and getting version numbers
        /// <summary>
        ///    <para>
        ///       The read-only property returns the "major" portion of the version
        ///       number.
        ///    </para>
        /// </summary>
        /// <value>
        ///    <para>
        ///       Read-only. Returns the major version.
        ///    </para>
        /// </value>
        /// <remarks>
        ///    <para>
        ///       For example, if the version number is 6.2, the "major" version is
        ///       6.
        ///    </para>
        /// </remarks>
        public int Major {
            get { return _Major; }
			set { _Major = value; }
        }
    
        /// <summary>
        ///    <para>
        ///       The read-only property returns the "minor" portion of the version
        ///       number.
        ///    </para>
        /// </summary>
        /// <value>
        ///    <para>
        ///       Read-only. Returns the minor version.
        ///    </para>
        /// </value>
        /// <remarks>
        ///    <para>
        ///       For example, if the version number is 6.2, the "minor" version is
        ///       2.
        ///    </para>
        /// </remarks>
        public int Minor {
            get { return _Minor; }
			set { _Minor = value; }
        }
    
        /// <summary>
        ///    <para>
        ///       The read-only property returns the "revision" portion of the
        ///       version number.
        ///    </para>
        /// </summary>
        /// <value>
        ///    <para>
        ///       Read-only. Returns the revision number. -1 is returned if the
        ///       revision number is undefined.
        ///    </para>
        /// </value>
        /// <remarks>
        ///    <para>
        ///        For example, if the
        ///       version number is 6.2.1.3, the "revision" number is 1. If the version number is
        ///       6.2, the "revision" number is undefined.
        ///    </para>
        /// </remarks>
        public int Revision {
            get { return _Revision; }
			set { _Revision = value; }
        }
    
        /// <summary>
        ///    <para>
        ///       The read-only property returns the "build" portion of the version
        ///       number.
        ///    </para>
        /// </summary>
        /// <value>
        ///    <para>
        ///       Read-only. Returns the build number. -1 is returned if the build
        ///       number is undefined.
        ///    </para>
        /// </value>
        /// <remarks>
        ///    <para>
        ///       For example, if the version number is 6.2.1.3, the "build" number
        ///       is 3. If the version number is 6.2, the "build" number is undefined.
        ///    </para>
        /// </remarks>
        public int Build {
            get { return _Build; }
			set { _Build = value; }
        }
     
        /// <summary>
        ///    <para>
        ///       Copies this Version object.
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       A copy of this Version object.
        ///    </para>
        /// </returns>
        public Object Clone() {
            MyVersion v = new MyVersion();
            v._Major = _Major;
            v._Minor = _Minor;
            v._Revision = _Revision;
            v._Build = _Build;
            return(v);
        }

        /// <summary>
        ///    <para>Compares this object with another Version object.</para>
        /// </summary>
        /// <returns>
        ///    <list type='table'><listheader>
        ///       <term>-1</term>
        ///    <description>If this version is older then <paramref name="version"/>.</description>
        /// </listheader>
        /// <item>
        ///    <term> 0</term>
        /// <description>If this version is the same as <paramref name="version"/>.</description>
        /// </item>
        /// <item>
        ///    <term> 1</term>
        /// <description>If this version is newer then <paramref name="version"/>.</description>
        /// </item>
        /// </list>
        /// </returns>
        /// <exception cref='ArgumentNullException'>if <paramref name="version"/> is null</exception>
        /// <remarks>
        ///    <para>The components of version in the decreasing order of importance
        ///       are: major, minor, revision, and build. An unknown component is assumed to be
        ///       smaller than any known component. For example,</para>
        ///    <para>version 1.1.* is older than version
        ///       1.1.1</para>
        ///    <para>version 1.1.* is older than version
        ///       1.1.2.3</para>
        ///    <para>version 1.1.2.* is older than version
        ///       1.1.2.4</para>
        ///    <para>version 1.2.5 is newer than version
        ///       1.2.3.4</para>
        /// </remarks>
        public int CompareTo(Object version)
        // @TODO
        {
            if (version == null)
                throw new ArgumentNullException("version");

            MyVersion v = (MyVersion) version;

            if (this._Major != v._Major)
                if (this._Major > v._Major)
                    return 1;
                else
                    return -1;

            if (this._Minor != v._Minor)
                if (this._Minor >= v._Minor)
                    return 1;
                else
                    return -1;

            if (this._Revision != v._Revision)
                if (this._Revision >= v._Revision)
                    return 1;
                else
                    return -1;

            if (this._Build != v._Build)
                if (this._Build >= v._Build)
                    return 1;
                else
                    return -1;

            return 0;
        }

        /// <summary>
        ///    <para>Checks if this object is equal to another Version object.</para>
        /// </summary>
        /// <param name='obj'>The object with which this version is to be compared.</param>
        /// <returns>
        /// <para>Returns <see langword='true '/> if every component of this version matches the corresponding component of <paramref name="version"/>. Otherwise, returns <see langword='false'/> .</para>
        /// </returns>
        /// <exception cref='ArgumentNullException'>if <paramref name="version"/> is null</exception>
        public override bool Equals(Object obj) {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (!(obj is MyVersion)) return(false);
            MyVersion v = (MyVersion) obj;
            // check that major, minor, revision & build numbers match
            if ((this._Major != v._Major) || 
                (this._Minor != v._Minor) || 
                (this._Revision != v._Revision) ||
                (this._Build != v._Build)) return(false);
            return(true);
        }

        public override int GetHashCode()
        {
            // Let's assume that most version numbers will be pretty small and just
            // OR some lower order bits together.

            int accumulator = 0;

            accumulator |= (this._Major & 0x0000000F) << 28;
            accumulator |= (this._Minor & 0x000000FF) << 20;
            accumulator |= (this._Revision & 0x000000FF) << 12;
            accumulator |= (this._Build & 0x00000FFF);

            return accumulator;
        }

        /// <overload>
        ///    <para>Overloaded. Returns a string representation of this Version.</para>
        /// </overload>
        /// <summary>
        ///    <para>Returns the default string representation of this Version
        ///       object. The format of the string returned is:</para>
        ///    <para>&lt;majorVersion&gt;.&lt;minorVersion&gt;[.&lt;revision&gt;[.&lt;build&gt;]]</para>
        ///    <para>Missing components of the Version are omitted in the
        ///       returned string.</para>
        /// </summary>
        /// <returns>
        ///    The default string representation of
        ///    this Version object.
        /// </returns>
        /// <example>
        ///    <para>For example, if Version was created using
        ///       the constructor Version(1,1), the returned string will be 1.1. If Version was
        ///       created using the constructor Version(1,3,4,2), the returned string will be
        ///       1.3.4.2.</para>
        /// </example>
        public override String ToString() {
            if (_Revision == -1) return(ToString(2));
            if (_Build == -1) return(ToString(3));
            return(ToString(4));
        }
        
        /// <summary>
        ///    <para>Returns the default string representation of
        ///       this Version that contains the specified number of
        ///       components. The format of the string returned is:</para>
        ///    <para>&lt;majorVersion&gt;.&lt;minorVersion&gt;[.&lt;revision&gt;[.&lt;build&gt;]]</para>
        /// </summary>
        /// <param name='fieldCount'>The number of components of the field count that are to be returned.</param>
        /// <returns>
        ///    <para>The default string representation of this Version object 
        ///       that contains the specified number of components. If <paramref name="fieldCount"/>is 0, an empty string (String.Empty) is returned.</para>
        /// </returns>
        /// <exception cref='ArgumentException'>if <paramref name="fieldCount "/>is more than the number of defined components of this Version or less than 0.</exception>
        /// <example>
        ///    <para>For example, if Version was created using the
        ///       constructor Version(1,3,5), the value returned for ToString(2) is 1.3. <see cref='System.ArgumentException'/>
        ///       will be thrown for ToString(4).</para>
        /// </example>
        public String ToString(int fieldCount) {
            switch (fieldCount) {
            case 0: 
                return(String.Empty);
            case 1: 
                return(String.Concat(_Major));
            case 2: 
                return(String.Concat(_Major,".",_Minor));
            case 3: 
                
                return( "" + _Major + "." + _Minor + "." + _Revision );
            case 4:
                
                return( "" + _Major + "." + _Minor + "." + _Revision + "." + _Build );
            default:
                throw new ArgumentException();
            }

        }

    }
}
