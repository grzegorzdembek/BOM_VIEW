namespace CAD_Agent.Adapters.SolidEdge
{
    internal class SePropertiesReader : IDisposable
    {
        private SeFilePropertySets _filePropertySets = null;
        private SePropertySets _docPropertySets = null;

        private readonly bool _isFileMode;
        private bool _disposed = false;

        private static Dictionary<string, string> _materialTranslations = null;
        private static readonly object _cacheLock = new();

        public SePropertiesReader(SeDocument document)
        {
            _isFileMode = false;
            _docPropertySets = (SePropertySets)document.Properties;
        }

        // Parts_Quantity. Int
        public int Quantity
        {
            get
            {
                object rawValue = _isFileMode ? GetCustomFileProperty(Constants.SeProperties.CustomSet, Constants.SeProperties.Quantity) : GetCustomDocProperty(Constants.SeProperties.CustomSet, Constants.SeProperties.Quantity);
                return (rawValue != null && int.TryParse(rawValue.ToString(), out int count)) ? count : 0;
            }
        }

        // 1. Type. String
        public string Type => GetPropertyString(Constants.SeProperties.CustomSet, Constants.SeProperties.Type);

        // 2. Title. String
        public string TitleEng => GetPropertyString(Constants.SeProperties.SummarySet, Constants.SeProperties.TitleEng);
        public string TitlePl => GetPropertyString(Constants.SeProperties.SummarySet, Constants.SeProperties.TitlePl);

        // 3. Provider. String
        public string Provider => GetPropertyString(Constants.SeProperties.CustomSet, Constants.SeProperties.Provider);

        // 4. Material Name. String
        public string MaterialName => GetPropertyString(Constants.SeProperties.CustomSet, Constants.SeProperties.MaterialName);

        // 5. Thickness. Double
        public double Thickness => GetDimensionAsDouble(Constants.SeProperties.CustomSet, Constants.SeProperties.Thickness);

        // 6. Width. Double
        public double SizeX => GetDimensionAsDouble(Constants.SeProperties.CustomSet, Constants.SeProperties.SizeX);

        // 7. Length. Double
        public double SizeY => GetDimensionAsDouble(Constants.SeProperties.CustomSet, Constants.SeProperties.SizeY);

        // 8. Mechanical Material. String
        public string MechanicalMaterial
        {
            get
            {
                object rawValue = _isFileMode ? GetCustomFileProperty(Constants.SeProperties.MechanicalModelingSet, Constants.SeProperties.Material) : GetCustomDocProperty(Constants.SeProperties.MechanicalModelingSet, Constants.SeProperties.Material);

                if (rawValue != null)
                {
                    EnsureMaterialsLoaded();
                    string material = rawValue.ToString();

                    if (_materialTranslations.TryGetValue(material, out string translatedMaterial))
                    {
                        return translatedMaterial;
                    }
                }

                return null;
            }
        }

        // 9. Finish. String
        public string Finish => GetPropertyString(Constants.SeProperties.CustomSet, Constants.SeProperties.Finish);

        // 10. Color. String 
        public string Color => GetPropertyString(Constants.SeProperties.CustomSet, Constants.SeProperties.Color);

        // 11. Mass
        // to do

        // 12. Dxf. String
        public string DxfDate => GetPropertyString(Constants.SeProperties.CustomSet, Constants.SeProperties.DxfDate);

        /*================================================================================================================*/
        private string GetPropertyString(string setName, string propName)
        {
            object rawValue = _isFileMode ? GetCustomFileProperty(setName, propName) : GetCustomDocProperty(setName, propName);
            return rawValue?.ToString();
        }

        private void SetProperty(string setName, string propName, object value)
        {
            if (_isFileMode)
            {
                SetCustomFileProperty(setName, propName, value);
            }
            else
            {
                SetCustomDocProperty(setName, propName, value);
            }
        }

        private object GetCustomFileProperty(string setName, string propName)
        {
            SeFileProperties properties = null;
            SeFileProperty property = null;

            try
            {
                properties = (SeFileProperties)_filePropertySets[setName];
                property = (SeFileProperty)properties[propName];
                return property.Value;
            }
            catch
            {
                return null;
            }
            finally
            {
                SeHelper.ReleaseCom(ref property);
                SeHelper.ReleaseCom(ref properties);
            }
        }

        private void SetCustomFileProperty(string setName, string propName, object value)
        {
            SeFileProperties properties = null;
            SeFileProperty property = null;

            try
            {
                properties = (SeFileProperties)_filePropertySets[setName];

                try
                {
                    property = (SeFileProperty)properties[propName];
                    property.Value = value;
                }
                catch
                {
                    property = (SeFileProperty)properties.Add(propName, value);
                }

                _filePropertySets.Save();
            }
            catch
            {
            }
            finally
            {
                SeHelper.ReleaseCom(ref property);
                SeHelper.ReleaseCom(ref properties);
            }
        }

        private object GetCustomDocProperty(string setName, string propName)
        {
            SeProperties properties = null;
            SeProperty property = null;

            try
            {
                properties = (SeProperties)_docPropertySets.Item(setName);
                property = (SeProperty)properties.Item(propName);
                dynamic dynProperty = property;
                return dynProperty.Value;
            }
            catch
            {
                return null;
            }
            finally
            {
                SeHelper.ReleaseCom(ref property);
                SeHelper.ReleaseCom(ref properties);
            }
        }

        private void SetCustomDocProperty(string setName, string propName, object value)
        {
            SeProperties properties = null;
            SeProperty property = null;

            try
            {
                properties = (SeProperties)_docPropertySets.Item(setName);

                for (int i = 1; i <= properties.Count; i++)
                {
                    SeProperty tempProp = null;

                    try
                    {
                        tempProp = (SeProperty)properties.Item(i);
                        dynamic dynProp = tempProp;

                        if (dynProp.Name == propName)
                        {
                            tempProp.Delete();
                            break;
                        }
                    }
                    finally
                    {
                        SeHelper.ReleaseCom(ref tempProp);
                    }
                }

                property = (SeProperty)properties.Add(propName, value);
            }
            finally
            {
                SeHelper.ReleaseCom(ref property);
                SeHelper.ReleaseCom(ref properties);
            }
        }

        private static void EnsureMaterialsLoaded()
        {
            if (_materialTranslations != null)
            {
                return;
            }

            lock (_cacheLock)
            {
                if (_materialTranslations != null)
                {
                    return;
                }

                _materialTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                string dllPath = Assembly.GetExecutingAssembly().Location;
                string basePath = Path.GetDirectoryName(dllPath);
                string materialsFile = Path.Combine(basePath, "materialy.txt");

                if (File.Exists(materialsFile))
                {
                    foreach (var line in File.ReadLines(materialsFile))
                    {
                        if (!string.IsNullOrWhiteSpace(line) && line.Contains(">"))
                        {
                            string[] parts = line.Split('>');
                            if (parts.Length == 2)
                            {
                                _materialTranslations[parts[0].Trim()] = parts[1].Trim();
                            }
                        }
                    }
                }
            }
        }

        private double GetDimensionAsDouble(string setName, string propName)
        {
            string rawValue = GetPropertyString(setName, propName);

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return 0.0;
            }

            string cleaned = rawValue.ToUpper()
                                     .Replace("MM", "")
                                     .Replace(" ", "")
                                     .Replace(",", ".");

            if (double.TryParse(cleaned, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            return 0.0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_isFileMode && _filePropertySets != null)
                {
                    try
                    {
                        _filePropertySets.Close();
                    }
                    catch
                    {
                    }

                    SeHelper.ReleaseCom(ref _filePropertySets);
                }
                else if (!_isFileMode && _docPropertySets != null)
                {
                    SeHelper.ReleaseCom(ref _docPropertySets);
                }
            }

            _filePropertySets = null;
            _docPropertySets = null;
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SePropertiesReader()
        {
            Dispose(false);
        }
    }
}