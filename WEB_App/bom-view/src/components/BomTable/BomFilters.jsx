export default function BomFilters({ currentView, setCurrentView }) {
  return (
    <div className="view-filters">
      <button 
        className={`view-btn ${currentView === 'STRUCTURE' ? 'active' : ''}`} 
        onClick={() => setCurrentView('STRUCTURE')}
      >
        Lista Strukturalna

      </button>
      <button 
        className={`view-btn ${currentView === 'ASSEMBLIES' ? 'active' : ''}`} 
        onClick={() => setCurrentView('ASSEMBLIES')}
      >
        Lista Złożenia

      </button>
      <button 
        className={`view-btn ${currentView === 'SHEETS' ? 'active' : ''}`} 
        onClick={() => setCurrentView('SHEETS')}
      >
        Lista Blach

      </button>
      <button 
        className={`view-btn ${currentView === 'PARTS' ? 'active' : ''}`} 
        onClick={() => setCurrentView('PARTS')}
      >
        Lista Części
        
      </button>
    </div>
  );
}