import { useState, useEffect } from 'react';
import { supabase } from '../services/supabaseClient'; 
import BomFilters from '../components/BomTable/BomFilters';
import BomTable from '../components/BomTable/BomTable';

export default function ProjectView() {
  const [bomData, setBomData] = useState([]);
  const [errorMsg, setErrorMsg] = useState(null);
  const [currentView, setCurrentView] = useState('STRUCTURE');

  useEffect(() => {
    async function fetchDataFromCloud() {
      const { data, error } = await supabase
        .from('bom_items') 
        .select('*');

      if (error) {
        setErrorMsg(error.message);
      } 
      else {
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
    <div style={{ width: '100%', maxWidth: '1600px', margin: '0 auto', padding: '20px' }}>
      <div style={{ padding: '20px 20px 0 20px', color: 'white' }}>
        <h2>Panel Projektu (BOM VIEW)</h2>
      </div>

      {errorMsg && <p style={{ color: 'red', padding: '0 20px' }}>Błąd bazy: {errorMsg}</p>}

      <BomFilters currentView={currentView} setCurrentView={setCurrentView} />

      <BomTable data={processedData} currentView={currentView} />
    </div>
  );
}