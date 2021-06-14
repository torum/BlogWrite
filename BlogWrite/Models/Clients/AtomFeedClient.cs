﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;

namespace BlogWrite.Models.Clients
{
    /// <summary>
    /// AtomFeedClient 
    /// Implements Atom Syndication Format
    /// </summary>
    class AtomFeedClient : BaseClient
    {
        public override async Task<List<EntryItem>> GetEntries(Uri entriesUrl)
        {            
            // Clear err msg.
            ClientErrorMessage = "";

            List<EntryItem> list = new List<EntryItem>();

            if (!(entriesUrl.Scheme.Equals("http") || entriesUrl.Scheme.Equals("https")))
            {
                ToDebugWindow("<< Invalid URI scheme:"
                    + Environment.NewLine
                    + entriesUrl.Scheme
                    + Environment.NewLine);

                ClientErrorMessage = "Invalid URI scheme (should be http or https): " + entriesUrl.Scheme;

                return list;
            }

            try
            {
                var HTTPResponseMessage = await _HTTPConn.Client.GetAsync(entriesUrl);

                if (HTTPResponseMessage.IsSuccessStatusCode)
                {
                    string s = await HTTPResponseMessage.Content.ReadAsStringAsync();

                    ToDebugWindow(">> HTTP Request GET "
                        //+ Environment.NewLine
                        + entriesUrl.AbsoluteUri
                        + Environment.NewLine + Environment.NewLine
                        + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                        + Environment.NewLine
                        + s + Environment.NewLine);


                    //System.Diagnostics.Debug.WriteLine("GET entries: " + s);
                    /*
            <?xml version="1.0" encoding="utf-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
             <title type="text">dive into mark</title>
             <subtitle type="html">
               A &lt;em&gt;lot&lt;/em&gt; of effort
               went into making this effortless
             </subtitle>
             <updated>2005-07-31T12:29:29Z</updated>
             <id>tag:example.org,2003:3</id>
             <link rel="alternate" type="text/html"
              hreflang="en" href="http://example.org/"/>
             <link rel="self" type="application/atom+xml"
              href="http://example.org/feed.atom"/>
             <rights>Copyright (c) 2003, Mark Pilgrim</rights>
             <generator uri="http://www.example.com/" version="1.0">
               Example Toolkit
             </generator>
             <entry>
               <title>Atom draft-07 snapshot</title>
               <link rel="alternate" type="text/html"
                href="http://example.org/2005/04/02/atom"/>
               <link rel="enclosure" type="audio/mpeg" length="1337"
                href="http://example.org/audio/ph34r_my_podcast.mp3"/>
               <id>tag:example.org,2003:3.2397</id>
               <updated>2005-07-31T12:29:29Z</updated>
               <published>2003-12-13T08:29:29-04:00</published>
               <author>
                 <name>Mark Pilgrim</name>
                 <uri>http://example.org/</uri>
                 <email>f8dy@example.com</email>
               </author>
               <contributor>
                 <name>Sam Ruby</name>
               </contributor>
               <contributor>
                 <name>Joe Gregorio</name>
               </contributor>
               <content type="xhtml" xml:lang="en"
                xml:base="http://diveintomark.org/">
                 <div xmlns="http://www.w3.org/1999/xhtml">
                   <p><i>[Update: The Atom draft is finished.]</i></p>
                 </div>
               </content>
             </entry>
            </feed>
                    */
                    var source = await HTTPResponseMessage.Content.ReadAsStreamAsync();

                    // Load XML
                    XmlDocument xdoc = new XmlDocument();
                    try
                    {
                        XmlReader reader = XmlReader.Create(source);
                        xdoc.Load(reader);
                        //xdoc.LoadXml(s);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("LoadXml failed: " + e.Message);

                        ToDebugWindow("<< Invalid XML returned:"
                            + Environment.NewLine
                            + e.Message
                            + Environment.NewLine);

                        ClientErrorMessage = "Invalid XML returned: " + e.Message;

                        return list;
                    }

                    XmlNamespaceManager atomNsMgr = new XmlNamespaceManager(xdoc.NameTable);
                    atomNsMgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                    atomNsMgr.AddNamespace("app", "http://www.w3.org/2007/app");

                    XmlNodeList entryList;
                    entryList = xdoc.SelectNodes("//atom:feed/atom:entry", atomNsMgr);
                    if (entryList == null)
                        return list;

                    foreach (XmlNode l in entryList)
                    {
                        AtomEntry ent = new AtomEntry("", this);
                        ent.Status = EntryItem.EntryStatus.esNormal;

                        FillEntryItemFromXML(ent, l, atomNsMgr);

                        list.Add(ent);
                    }
                }
                // HTTP non 200 status code
                else
                {
                    var contents = await HTTPResponseMessage.Content.ReadAsStringAsync();

                    if (contents != null)
                    {
                        ToDebugWindow(">> HTTP Request GET "
                            //+ Environment.NewLine
                            + entriesUrl.AbsoluteUri
                            + Environment.NewLine + Environment.NewLine
                            + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                            + Environment.NewLine
                            + contents + Environment.NewLine);
                    }

                    ClientErrorMessage = "HTTP request failed: " + HTTPResponseMessage.StatusCode.ToString();
                }

            }
            // Internet connection errors
            catch (System.Net.Http.HttpRequestException e)
            {
                Debug.WriteLine("<< HttpRequestException: " + e.Message);

                ToDebugWindow(" << HttpRequestException: "
                    + Environment.NewLine
                    + e.Message
                    + Environment.NewLine);

                ClientErrorMessage = "HTTP request error: " + e.Message;
            }
            catch (Exception e)
            {
                Debug.WriteLine("HTTP error: " + e.Message);

                ToDebugWindow("<< HTTP error:"
                    + Environment.NewLine
                    + e.Message
                    + Environment.NewLine);

                ClientErrorMessage = "HTTP error: " + e.Message;
            }

            return list;
        }

        private async void FillEntryItemFromXML(AtomEntry entItem, XmlNode entryNode, XmlNamespaceManager atomNsMgr)
        {

            AtomEntry entry = await CreateAtomEntryFromXML(entryNode, atomNsMgr);

            entItem.Name = entry.Name;
            //entItem.ID = entry.ID;
            entItem.EntryId = entry.EntryId;
            entItem.EditUri = entry.EditUri;
            entItem.AltHtmlUri = entry.AltHtmlUri;
            entItem.Published = entry.Published;
            entItem.Summary = entry.Summary;
            entItem.SummaryPlainText = entry.SummaryPlainText;
            entItem.Content = entry.Content;
            entItem.ContentType = entry.ContentType;
            // entItem.EntryBody = entry;

            entItem.Status = entry.Status;

            if (entItem.ContentType == EntryItem.ContentTypes.textHtml)
            {
                // gets image Uri
                entItem.ImageUri = await GetImageUriFromHtml(entItem.Content);

                // TODO: this is just a test.
                if (entItem.ImageUri != null)
                {
                    Byte[] bytes = await this.GetImage(entItem.ImageUri);
                    if (bytes != Array.Empty<byte>())
                    {
                        if (Application.Current == null) { return; }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            entItem.Image = BitmapImageFromBytes(bytes);
                        });
                    }
                }
            }

        }

        private async Task<AtomEntry> CreateAtomEntryFromXML(XmlNode entryNode, XmlNamespaceManager atomNsMgr)
        {

            XmlNode entryTitle = entryNode.SelectSingleNode("atom:title", atomNsMgr);
            if (entryTitle == null)
            {
                System.Diagnostics.Debug.WriteLine("atom:title: is null. ");
                //return;
            }

            XmlNode entryID = entryNode.SelectSingleNode("atom:id", atomNsMgr);
            if (entryID == null)
            {
                System.Diagnostics.Debug.WriteLine("atom:id: is null. ");
                //return;
            }

            XmlNodeList entryLinkUris = entryNode.SelectNodes("atom:link", atomNsMgr);
            string relAttr;
            string hrefAttr;
            string typeAttr;
            Uri editUri = null;
            Uri altUri = null;
            if (entryLinkUris == null)
            {
                System.Diagnostics.Debug.WriteLine("atom:link: is null. ");
                //continue;
            }
            else
            {
                foreach (XmlNode u in entryLinkUris)
                {
                    relAttr = (u.Attributes["rel"] != null) ? u.Attributes["rel"].Value : "";
                    hrefAttr = (u.Attributes["href"] != null) ? u.Attributes["href"].Value : "";
                    typeAttr = (u.Attributes["type"] != null) ? u.Attributes["type"].Value : "";

                    if (!string.IsNullOrEmpty(hrefAttr))
                    {

                        switch (relAttr)
                        {
                            case "edit":
                                try
                                {
                                    editUri = new Uri(hrefAttr);
                                    break;
                                }
                                catch
                                {
                                    break;
                                }
                            case "alternate":
                                try
                                {
                                    if (!string.IsNullOrEmpty(typeAttr))
                                    {
                                        if (typeAttr == "text/html")
                                        {
                                            altUri = new Uri(hrefAttr);
                                        }
                                    }
                                    break;
                                }
                                catch
                                {
                                    break;
                                }
                            case "": //same as alternate
                                try
                                {
                                    if (!string.IsNullOrEmpty(typeAttr))
                                    {
                                        if (typeAttr == "text/html")
                                        {
                                            altUri = new Uri(hrefAttr);
                                        }
                                    }
                                    else
                                    {
                                        // I am not happy but let's assume it is html.
                                        altUri = new Uri(hrefAttr);
                                    }
                                    break;
                                }
                                catch
                                {
                                    break;
                                }
                        }
                    }
                }
            }

            // TODO:
            //updated
            //published
            //app:edited
            //summary
            //category

            AtomEntry entry = new AtomEntry("", this);
            // TODO:
            //AtomEntryHatena
            /*
            // Hatena's formatted-content
            XmlNode formattedContent = entryNode.SelectSingleNode("hatena:formatted-content", atomNsMgr);
            if (formattedContent != null)
            {
                entry.FormattedContent = formattedContent.InnerText;
            }
            */

            entry.Name = (entryTitle != null) ? entryTitle.InnerText : "";
            entry.EntryId = (entryID != null) ? entryID.InnerText : "";
            entry.EditUri = editUri;
            entry.AltHtmlUri = altUri;

            XmlNode cont = entryNode.SelectSingleNode("atom:content", atomNsMgr);
            if (cont == null)
            {
                System.Diagnostics.Debug.WriteLine("//atom:content is null.");
            }
            else
            {
                string contype = cont.Attributes["type"].Value;
                if (!string.IsNullOrEmpty(contype))
                {
                    entry.ContentTypeString = contype;

                    switch (contype)
                    {
                        case "text":
                            entry.ContentType = EntryItem.ContentTypes.text;
                            break;
                        case "html":
                            entry.ContentType = EntryItem.ContentTypes.textHtml;
                            break;
                        case "xhtml":
                            entry.ContentType = EntryItem.ContentTypes.textHtml;
                            break;
                        case "text/plain":
                            entry.ContentType = EntryItem.ContentTypes.text;
                            break;
                        case "text/html":
                            entry.ContentType = EntryItem.ContentTypes.textHtml;
                            break;
                        case "text/x-markdown":
                            entry.ContentType = EntryItem.ContentTypes.markdown;
                            break;
                        case "text/x-hatena-syntax":
                            entry.ContentType = EntryItem.ContentTypes.hatena;
                            break;
                        default:
                            entry.ContentType = EntryItem.ContentTypes.text;
                            break;
                    }
                }

                entry.Content = cont.InnerText;
            }

            XmlNode sum = entryNode.SelectSingleNode("atom:summary", atomNsMgr);
            if (sum != null)
            {
                entry.Summary = await StripStyleAttributes(sum.InnerText);
                //entry.ContentType = EntryFull.ContentTypes.textHtml;

                if (!string.IsNullOrEmpty(sum.InnerText))
                {
                    //entry.SummaryPlainText = await StripHtmlTags(sum.InnerText);

                    entry.SummaryPlainText = Truncate(sum.InnerText, 78);
                }
            }
            else
            {
                string s = entry.Content;

                if (!string.IsNullOrEmpty(s))
                {
                    if (entry.ContentType == EntryFull.ContentTypes.textHtml)
                    {
                        s = await StripHtmlTags(s);
                        entry.SummaryPlainText = Truncate(s, 78);
                    }
                    else if (entry.ContentType == EntryFull.ContentTypes.text)
                    {
                        entry.SummaryPlainText = Truncate(s, 78);
                    }
                }
            }

            XmlNode entryPublished = entryNode.SelectSingleNode("atom:published", atomNsMgr);
            if (entryPublished != null)
            {
                if (!string.IsNullOrEmpty(entryPublished.InnerText))
                {
                    entry.Published = XmlConvert.ToDateTime(entryPublished.InnerText, XmlDateTimeSerializationMode.Utc);
                }
            }

            /*
            XmlNode entryUpdated = entryNode.SelectSingleNode("atom:updated", atomNsMgr);
            if (entryUpdated != null)
            {
                if (!string.IsNullOrEmpty(entryUpdated.InnerText))
                {
                     = XmlConvert.ToDateTime(entryUpdated.InnerText, XmlDateTimeSerializationMode.Utc);
                }
            }
            */


            /*
            //app:control/app:draft(yes/no)
            XmlNode entryDraft = entryNode.SelectSingleNode("app:control/app:draft", atomNsMgr);
            if (entryDraft == null) System.Diagnostics.Debug.WriteLine("app:draft: is null.");

            string draft = entryDraft?.InnerText;
            entry.IsDraft = (String.Compare(draft, "yes", true) == 0) ? true : false;
            */

            entry.Status = EntryItem.EntryStatus.esNormal;


            return entry;
        }

    }

}
