using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.Sites;
using Sitecore.Web;
using System;

namespace Sitecore.Support.Resources.Media
{
    public class MediaProvider : Sitecore.Resources.Media.MediaProvider
    {
        private MediaConfig config;
        private MediaCache cache = new MediaCache();
        private MediaCreator creator = new MediaCreator();
        private ImageEffects effects = new ImageEffects();
        private MimeResolver mimeResolver = new MimeResolver();

        public override string GetMediaUrl(MediaItem item, MediaUrlOptions options)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(options, "options");
            Assert.IsTrue(this.Config.MediaPrefixes[0].Length > 0, "media prefixes are not configured properly.");
            string text = this.MediaLinkPrefix;
            if (options.AbsolutePath)
            {
                text = options.VirtualFolder + text;
            }
            else if (text.StartsWith("/", StringComparison.InvariantCulture))
            {
                text = StringUtil.Mid(text, 1);
            }
            text = MainUtil.EncodePath(text, '/');
            if (options.AlwaysIncludeServerUrl)
            {
                text = FileUtil.MakePath(string.IsNullOrEmpty(options.MediaLinkServerUrl) ? GetServerUrlElement(Sitecore.Context.Site.SiteInfo, options) : options.MediaLinkServerUrl, text, '/');
            }
            string text2 = StringUtil.GetString(new string[]
            {
        options.RequestExtension,
        item.Extension,
        "ashx"
            });
            text2 = StringUtil.EnsurePrefix('.', text2);
            string text3 = options.ToString();
            if (text3.Length > 0)
            {
                text2 = text2 + "?" + text3;
            }
            string text4 = "/sitecore/media library/";
            string path = item.InnerItem.Paths.Path;
            string text5;
            if (options.UseItemPath && path.StartsWith(text4, StringComparison.OrdinalIgnoreCase))
            {
                text5 = StringUtil.Mid(path, text4.Length);
            }
            else
            {
                text5 = item.ID.ToShortID().ToString();
            }
            text5 = MainUtil.EncodePath(text5, '/');
            text5 = text + text5 + (options.IncludeExtension ? text2 : string.Empty);
            if (!options.LowercaseUrls)
            {
                return text5;
            }
            return text5.ToLowerInvariant();
        }

        protected virtual string GetServerUrlElement(SiteInfo siteInfo, MediaUrlOptions options)
        {
            SiteContext site = Context.Site;
            string value = (site != null) ? site.Name : string.Empty;
            string hostName = WebUtil.GetHostName();
            string result = options.AlwaysIncludeServerUrl ? WebUtil.GetServerUrl() : string.Empty;
            if (siteInfo == null)
            {
                return result;
            }
            string text = (!string.IsNullOrEmpty(siteInfo.HostName) && !string.IsNullOrEmpty(hostName) && siteInfo.Matches(hostName)) ? hostName : StringUtil.GetString(new string[]
            {
        this.GetTargetHostName(siteInfo),
        hostName
            });
            string @string = StringUtil.GetString(new string[]
            {
        siteInfo.Scheme,
        WebUtil.GetScheme()
            });
            int num = MainUtil.GetInt(siteInfo.Port, WebUtil.GetPort());
            int port = WebUtil.GetPort();
            int @int = MainUtil.GetInt(siteInfo.ExternalPort, num);
            if (@int != num)
            {
                if (options.AlwaysIncludeServerUrl)
                {
                    result = ((@int == 80) ? string.Format("{0}://{1}", @string, WebUtil.GetHostName()) : string.Format("{0}://{1}:{2}", @string, WebUtil.GetHostName(), @int));
                }
                num = @int;
            }
            if (!options.AlwaysIncludeServerUrl && siteInfo.Name.Equals(value, StringComparison.OrdinalIgnoreCase) && hostName.Equals(text, StringComparison.OrdinalIgnoreCase))
            {
                return result;
            }
            if (string.IsNullOrEmpty(text) || text.IndexOf('*') >= 0)
            {
                return result;
            }
            string scheme = WebUtil.GetScheme();
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase;
            if (text.Equals(hostName, comparisonType) && num == port && @string.Equals(scheme, comparisonType))
            {
                return result;
            }
            string text2 = @string + "://" + text;
            if (num > 0 && num != 80)
            {
                text2 = text2 + ":" + num;
            }
            return text2;
        }

        protected virtual string GetTargetHostName(SiteInfo siteInfo)
        {
            Assert.ArgumentNotNull(siteInfo, "siteInfo");
            if (!string.IsNullOrEmpty(siteInfo.TargetHostName))
            {
                return siteInfo.TargetHostName;
            }
            string hostName = siteInfo.HostName;
            if (hostName.IndexOfAny(new char[]
            {
        '*',
        '|'
            }) < 0)
            {
                return hostName;
            }
            return string.Empty;
        }
    }
}
