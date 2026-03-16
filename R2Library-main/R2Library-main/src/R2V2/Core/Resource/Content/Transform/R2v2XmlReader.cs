namespace R2V2.Core.Resource.Content.Transform
{
    //
    //XmlValidatingReader is obsolete, do not use!	-DRJ
    //


    /*public class R2V2XmlReader : XmlValidatingReader, IDisposable // XmlValidatingReader
    {
        //This class was created to avoid unnecessary processing of external entities in book.xml files
        //This yields a significant performance boost when transforming content -DRJ

        public R2V2XmlReader(XmlReader xmlReader, bool ignoreExternalEntities) : base(xmlReader)
        {
            if(ignoreExternalEntities) {
                EntityHandling = EntityHandling.ExpandCharEntities; //Expand char entities only; do not expand external entities
            }

            ValidationEventHandler += R2V2XmlReaderValidationEventHandler;
        }

        public override void ResolveEntity()
        {
            //Don't resolve entities
        }

        public void Dispose() {
            ValidationEventHandler -= R2V2XmlReaderValidationEventHandler;
            base.Dispose(true);
        }

        private void R2V2XmlReaderValidationEventHandler (object sender, ValidationEventArgs args)
        {
            //Ignore XML validation errors, since DTD we are using does not currently validate.
            //This was the existing behavior from the previous version of the code -DRJ
        }
    }*/
}