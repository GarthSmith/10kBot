/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2008 by AG-Software 											 *
 * All Rights Reserved.																 *
 * Contact information for AG-Software is available at http://www.ag-software.de	 *
 *																					 *
 * Licence:																			 *
 * The agsXMPP SDK is released under a dual licence									 *
 * agsXMPP can be used under either of two licences									 *
 * 																					 *
 * A commercial licence which is probably the most appropriate for commercial 		 *
 * corporate use and closed source projects. 										 *
 *																					 *
 * The GNU Public License (GPL) is probably most appropriate for inclusion in		 *
 * other open source projects.														 *
 *																					 *
 * See README.html for details.														 *
 *																					 *
 * For general enquiries visit our website at:										 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */ 
#if !CF
using System;
using System.IO;
using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.iq.vcard
{
	/// <summary>
	/// Vcard Photo
	/// When you dont want System.Drawing in the Lib just remove the photo stuff
	/// </summary>
	public class Photo : Element
	{
		// <!-- Photograph property. Value is either a BASE64 encoded
		// binary value or a URI to the external content. -->
		// <!ELEMENT PHOTO ((TYPE, BINVAL) | EXTVAL)>	
		#region << Constructors >>
		public Photo()
		{
			this.TagName	= "PHOTO";
			this.Namespace	= Uri.VCARD;
		}
		public Photo(string url) : this()
		{
			SetImage(url);
		}
		#endregion
				
		/// <summary>
		/// The Media Type, Only available when BINVAL
		/// </summary>
		public string Type
		{
			//<TYPE>image</TYPE>
			get { return GetTag("TYPE"); }
			set { SetTag("TYPE", value); }
		}

		/// <summary>
		/// Sets the URL of an external image
		/// </summary>
		/// <param name="url"></param>
		public void SetImage(string url)
		{
			SetTag("EXTVAL", url);
		}

        /*
		/// <summary>
		/// Sets a internal Image
		/// </summary>
		/// <param name="image"></param>
		public void SetImage(Image image)
		{
			Image = image;
		}
        */
    }
}
#endif