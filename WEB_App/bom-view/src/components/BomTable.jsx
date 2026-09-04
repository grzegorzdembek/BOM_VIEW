import { useState, useEffect } from 'react';
import { supabase } from '../supabaseClient'; 

export default function BomTable() {
  const [bomData, setBomData] = useState([]);
  const [errorMsg, setErrorMsg] = useState(null);
  const [currentView, setCurrentView] = useState('STRUCTURE');

  useEffect(() => {
    async function fetchDataFromCloud() {
      const { data, error } = await supabase
        .from('bom_items') 
        .select('*');

      if (error) {
        console.error('Błąd połączenia z magazynem:', error.message);
        setErrorMsg(error.message);
      } else {
        const sortedData = data.sort((a, b) => {
          const aParts = a.structure_id.split('.').map(Number);
          const bParts = b.structure_id.split('.').map(Number);
          
          for (let i = 0; i < Math.max(aParts.length, bParts.length); i++) {
            const aVal = aParts[i] || 0;
            const bVal = bParts[i] || 0;
            if (aVal !== bVal) return aVal - bVal;
          }
          return 0;
        });

        setBomData(sortedData); 
      }
    }

    fetchDataFromCloud(); 
  }, []); 

  let processedData = bomData.filter((part) => {
    if (currentView === 'STRUCTURE') return true; 
    if (currentView === 'ASSEMBLIES') return part.type === 'A'; 
    if (currentView === 'SHEETS') return part.type === 'B';
    if (currentView === 'PARTS') return part.type === 'C';
    return true;
  });

  if (currentView !== 'STRUCTURE') {
    const uniquePartsMap = new Map();
    processedData.forEach((part) => {
      if (!uniquePartsMap.has(part.part_number)) {
        uniquePartsMap.set(part.part_number, part);
      }
    });
    processedData = Array.from(uniquePartsMap.values());
  }

  return (
    <div className="table-container">
      <div style={{ padding: '20px 20px 0 20px' }}>
        <h2>Podgląd Danych </h2>
      </div>

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

      {errorMsg && <p style={{ color: 'red', padding: '0 20px' }}>Błąd bazy: {errorMsg}</p>}

      <div className="table-scroll-wrapper">
        <table className="bom-table">
          <thead>
            <tr>
              {currentView === 'STRUCTURE' && <th>Lp_S</th>}
              {currentView === 'STRUCTURE' && <th>Ilość_S</th>}
              <th>Lp_P</th>
              <th>Ilość_P</th>
              <th>Typ</th>
              <th>Numer części</th>
              <th>Tytuł</th>
              <th>Dostawca</th>
              <th>Materiał_nazwa</th>
              <th>Grubość</th>
              <th>Szerokość</th>
              <th>Długość</th>
              <th>Materiał</th>
              <th>Klasa</th>
              <th>Wykończenie</th>
              <th>Kolor</th>
              <th>Masa</th>
              <th>DXF Date</th>
              <th>Miniatura</th>
              <th>Projekt</th>
            </tr>
          </thead>
          <tbody>
            {processedData.length === 0 ? (
              <tr>
                <td colSpan={currentView === 'STRUCTURE' ? "20" : "18"} style={{ textAlign: 'center', padding: '30px' }}>
                  Brak części pasujących do wybranego widoku.
                </td>
              </tr>
            ) : (
              processedData.map((part) => (
                <tr key={part.structure_id}>
                  {currentView === 'STRUCTURE' && <td>{part.structure_id}</td>}
                  {currentView === 'STRUCTURE' && <td>{part.structure_quantity}</td>}
                  <td>{part.parts_id}</td>
                  <td>{part.parts_quantity}</td>
                  <td>{part.type}</td>
                  <td>{part.part_number}</td>
                  <td>{part.title}</td>
                  <td>{part.provider}</td>
                  <td>{part.material_name}</td>
                  <td>{part.thickness}</td>
                  <td>{part.size_x}</td>
                  <td>{part.size_y}</td>
                  <td>{part.mechanical_material}</td>
                  <td>{part.class}</td>
                  <td>{part.finish}</td>
                  <td>{part.color}</td>
                  <td>{part.mass}</td>
                  <td>{part.dxf_date}</td>
                  <td>{part.thumbnail}</td>
                  <td>{part.project_name}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}