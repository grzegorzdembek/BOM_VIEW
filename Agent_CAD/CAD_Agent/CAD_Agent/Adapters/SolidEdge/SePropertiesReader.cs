namespace CAD_Agent.Adapters.SolidEdge
{
    public class SePropertiesReader : IDisposable
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

        public string Color => GetPropertyString(Constants.SeProperties.CustomSet, Constants.SeProperties.Color);

        public string Finish => GetPropertyString(Constants.SeProperties.CustomSet, Constants.SeProperties.Finish);

        public string TitleEng => GetPropertyString(Constants.SeProperties.SummarySet, Constants.SeProperties.TitleEng);

        public string TitlePl => GetPropertyString(Constants.SeProperties.SummarySet, Constants.SeProperties.TitlePl);

        public string Type
        {
            get
            {
                return GetPropertyString(Constants.SeProperties.CustomSet, Constants.SeProperties.Type);
            }
            set
            {
                SetProperty(Constants.SeProperties.CustomSet, Constants.SeProperties.Type, value);
            }
        }

        public int Status
        {
            get
            {
                object val = _isFileMode ? GetCustomFileProperty(Constants.SeProperties.ExtendedSummarySet, Constants.SeProperties.Status) : GetCustomDocProperty(Constants.SeProperties.ExtendedSummarySet, Constants.SeProperties.Status);
                return val != null ? (int)val : -1;
            }
        }

        public string Thickness
        {
            get
            {
                object rawValue = _isFileMode ? GetCustomFileProperty(Constants.SeProperties.CustomSet, Constants.SeProperties.Thickness) : GetCustomDocProperty(Constants.SeProperties.CustomSet, Constants.SeProperties.Thickness);

                if (rawValue != null)
                {
                    string thickness = rawValue.ToString().Replace("mm", "").Replace(" ", "").Trim();
                    thickness = thickness.Replace('.', ',');

                    if (thickness.Contains(","))
                    {
                        thickness = thickness.TrimEnd('0').TrimEnd(',');
                    }

                    return thickness.Replace(',', '_');
                }

                return null;
            }
        }

        public string Material
        {
            get
            {
                object rawValue = _isFileMode ? GetCustomFileProperty(Constants.SeProperties.MechanicalModeling, Constants.SeProperties.Material) : GetCustomDocProperty(Constants.SeProperties.MechanicalModeling, Constants.SeProperties.Material);

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

        public string MaterialName => GetPropertyString(Constants.SeProperties.CustomSet, Constants.SeProperties.MaterialName);

        public int Count
        {
            get
            {
                object rawValue = _isFileMode ? GetCustomFileProperty(Constants.SeProperties.CustomSet, Constants.SeProperties.Count) : GetCustomDocProperty(Constants.SeProperties.CustomSet, Constants.SeProperties.Count);
                return (rawValue != null && int.TryParse(rawValue.ToString(), out int count)) ? count : 0;
            }
            set
            {
                SetProperty(Constants.SeProperties.CustomSet, Constants.SeProperties.Count, value);
            }
        }

        public string DxfDate => GetPropertyString(Constants.SeProperties.CustomSet, Constants.SeProperties.DxfDate);

        public string SizeX => GetPropertyString(Constants.SeProperties.CustomSet, Constants.SeProperties.SizeX);

        public string SizeY => GetPropertyString(Constants.SeProperties.CustomSet, Constants.SeProperties.SizeY);

        public bool HasType => !string.IsNullOrEmpty(Type);

        public bool HasStatus => Status >= 0;

        public bool HasThickness => !string.IsNullOrEmpty(Thickness);

        public bool HasMaterial => !string.IsNullOrEmpty(Material);

        public bool HasCount => Count > 0;

        public bool HasDxfDate => !string.IsNullOrEmpty(DxfDate);

        public bool IsStatusAvailable => Status == 0;

        public bool IsTypeA => Type == Constants.SePartTypes.Assembly;

        public bool IsTypeB => Type == Constants.SePartTypes.SheetMetal;

        public bool IsTypeC => Type == Constants.SePartTypes.Part;

        public bool IsTypeK => Type == Constants.SePartTypes.Steelmaking;

        public bool IsTypeH => Type == Constants.SePartTypes.Commercial;

        public bool IsTypeN => Type == Constants.SePartTypes.Standard;

        public void UpdateDxfDate()
        {
            SetProperty(Constants.SeProperties.CustomSet, Constants.SeProperties.DxfDate, DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));
        }

        public void ClearDxfDate()
        {
            SetProperty(Constants.SeProperties.CustomSet, Constants.SeProperties.DxfDate, string.Empty);
        }

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