using System;
using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.merge.xml.lift
{
	public class LexEntryContextGenerator :IGenerateContextDescriptor
	{
		public string GenerateContextDescriptor(string mergeElement)
		{
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			var node = doc.FirstChild.Attributes.GetNamedItem("guid");
			if(node!=null)
			{
				return String.Format("lift/entry[@guid='{0}']", node.Value);
			}
			node = doc.FirstChild.Attributes.GetNamedItem("id");
			if(node!=null)
			{
				return String.Format("lift/entry[@id='{0}']", node.Value);
			}
			throw new ApplicationException("Could not get guid or id attribute out of "+mergeElement);

		}
	}

	public class LiftEntryMergingStrategy : IMergeStrategy
	{
		private XmlMerger _entryMerger;

		public LiftEntryMergingStrategy(MergeSituation mergeSituation)
		{
			_entryMerger = new XmlMerger(mergeSituation);

			//now customize the XmlMerger with LIFT-specific info

			var elementStrategy =AddKeyedElementType("entry", "id", false);
			elementStrategy.ContextDescriptorGenerator = new LexEntryContextGenerator();

			AddKeyedElementType("sense", "id", true);
			AddKeyedElementType("form", "lang", false);
			AddKeyedElementType("gloss", "lang", false);
			AddKeyedElementType("field", "type", false);

			AddSingletonElementType("text");
			AddSingletonElementType("grammatical-info");
			AddSingletonElementType("lexical-unit" );
			AddSingletonElementType("citation" );
			AddSingletonElementType("definition");
			AddSingletonElementType("label");
			AddSingletonElementType("usage");
			AddSingletonElementType("header");
			AddSingletonElementType("description"); // in header
			AddSingletonElementType("ranges"); // in header
			AddSingletonElementType("fields"); // in header

			//enhance: don't currently have a way of limitting etymology/form to a single instance but not multitext/form

			AddSingletonElementType("main"); //reversal/main

		}

		private ElementStrategy AddKeyedElementType(string name, string attribute, bool orderOfTheseIsRelevant)
		{
			ElementStrategy strategy = new ElementStrategy(orderOfTheseIsRelevant);
			strategy.MergePartnerFinder = new FindByKeyAttribute(attribute);
			_entryMerger.MergeStrategies.SetStrategy(name, strategy);
			return strategy;
		}

		private ElementStrategy AddSingletonElementType(string name)
		{
			ElementStrategy strategy = new ElementStrategy(false);
			strategy.MergePartnerFinder = new FindFirstElementWithSameName();
			_entryMerger.MergeStrategies.SetStrategy(name, strategy);
			return strategy;
		}

		public string MakeMergedEntry(IMergeEventListener listener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			XmlNode n = _entryMerger.Merge(listener, ourEntry, theirEntry, commonEntry);
			return n.OuterXml;
		}
	}

	// JohnT: not currently used, and not updated to new interface.
	//public class FindMatchingExampleTranslation : IFindNodeToMerge
	//{
	//    public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn)
	//    {
	//        //todo: this may choke with multiples of the same type!

	//        //enhance... if we could rely on creation date + type, that'd help, but if
	//        // it was automatically done, multiple could come in with the same datetime

	//        string type = XmlUtilities.GetOptionalAttributeString(nodeToMatch, "type");
	//        string xpath;
	//        if (string.IsNullOrEmpty(type))
	//        {
	//            xpath = String.Format("translation[not(@type)]");
	//        }
	//        else
	//        {
	//            xpath = string.Format("translation[@type='{0}']", type);
	//        }
	//        XmlNode n= parentToSearchIn.SelectSingleNode(xpath);
	//        if (n != null)
	//        {
	//            return n;
	//        }
	//        else
	//        {
	//            //enhance: can we find one with a matching multitext? Maybe one guy just changed the type.
	//            return null;
	//        }
	//    }

	//}
}