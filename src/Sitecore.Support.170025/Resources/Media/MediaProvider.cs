using Microsoft.Extensions.DependencyInjection;
using Sitecore;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
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
        [Obsolete]
        public MediaProvider() : base(ServiceLocator.ServiceProvider.GetRequiredService<BaseFactory>())
        {
        }

        public MediaProvider(BaseFactory factory) : base(factory)
        {
        }

        [NotNull]
        public override string GetMediaUrl([NotNull] MediaItem item, [NotNull] MediaUrlOptions options)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(options, "options");

            Assert.IsTrue(this.Config.MediaPrefixes[0].Length > 0, "media prefixes are not configured properly.");
            string prefix = this.MediaLinkPrefix;

            if (options.AbsolutePath)
            {
                prefix = options.VirtualFolder + prefix;
            }
            else if (prefix.StartsWith("/", StringComparison.InvariantCulture))
            {
                prefix = StringUtil.Mid(prefix, 1);
            }

            prefix = MainUtil.EncodePath(prefix, '/');

            if (options.AlwaysIncludeServerUrl)
            {
                prefix = FileUtil.MakePath(string.IsNullOrEmpty(options.MediaLinkServerUrl) ? GetServerUrlElement(Context.Site.SiteInfo, options) : options.MediaLinkServerUrl, prefix, '/');
            }

            string extension = StringUtil.GetString(options.RequestExtension, item.Extension, Constants.AshxExtension);

            extension = StringUtil.EnsurePrefix('.', extension);

            string parameters = options.ToString();

            if (options.AlwaysAppendRevision)
            {
              var rev = Guid.Parse(item.InnerItem.Statistics.Revision).ToString("N");
              parameters = string.IsNullOrEmpty(parameters) ? "rev=" + rev : parameters + "&rev=" + rev;
            }

            if (parameters.Length > 0)
            {
                extension += "?" + parameters;
            }

            string mediaRoot = Constants.MediaLibraryPath + "/";
            string itemPath = item.InnerItem.Paths.Path;

            string path;

            if (options.UseItemPath
                && itemPath.StartsWith(mediaRoot, StringComparison.OrdinalIgnoreCase))
            {
                path = StringUtil.Mid(itemPath, mediaRoot.Length);
            }
            else
            {
                path = item.ID.ToShortID().ToString();
            }

            path = MainUtil.EncodePath(path, '/');
            path = prefix + path + (options.IncludeExtension ? extension : string.Empty);
            return options.LowercaseUrls ? path.ToLowerInvariant() : path;
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
