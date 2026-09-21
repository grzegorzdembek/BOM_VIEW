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

  const handleSortStructure = () => {
    const dataCopy = JSON.parse(JSON.stringify(bomData));

    const nodeMap = new Map();
    const root = { children: [] };

    dataCopy.forEach(item => {
      item.children = [];
      nodeMap.set(item.structure_id, item);
    });

    dataCopy.forEach(item => {
      const parts = item.structure_id.split('.');
      if (parts.length === 1) {
        root.children.push(nodeMap.get(item.structure_id)); 
      } else {
        const parentId = parts.slice(0, -1).join('.');
        const parent = nodeMap.get(parentId);
        if (parent) {
          parent.children.push(nodeMap.get(item.structure_id));
        } else {
          root.children.push(nodeMap.get(item.structure_id)); 
        }
      }
    });

    const result = [];
    
    const sortAndFlatten = (node, prefix) => {
      node.children.sort((a, b) => (a.part_number || '').localeCompare(b.part_number || ''));
      
      node.children.forEach((child, index) => {
        const newId = prefix ? `${prefix}.${index + 1}` : `${index + 1}`;
        child.structure_id = newId; 
        
        const { children, ...cleanItem } = child; 
        result.push(cleanItem);
        
        sortAndFlatten(child, newId); 
      });
    };

    sortAndFlatten(root, ''); 
    
    setBomData(result);
  };
  // ==========================================

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
    <div style={{ width: '100%', maxWidth: '100%', margin: '0 auto', padding: '20px 40px' }}>
      
      <div style={{ padding: '0 0 20px 0', color: 'white' }}>
        <h2>Panel Projektu (BOM VIEW)</h2>
      </div>

      <div style={{ paddingBottom: '20px', display: 'flex', gap: '10px' }}>
        <button 
          onClick={handleSortStructure}
          disabled={currentView !== 'STRUCTURE'} 
          style={{
            backgroundColor: currentView === 'STRUCTURE' ? '#3b82f6' : '#334155', 
            color: currentView === 'STRUCTURE' ? '#ffffff' : '#94a3b8',
            border: 'none', padding: '10px 16px', borderRadius: '6px', 
            cursor: currentView === 'STRUCTURE' ? 'pointer' : 'not-allowed',
            fontWeight: '600', display: 'flex', alignItems: 'center', gap: '8px',
            transition: 'background-color 0.2s'
          }}
        >
          <svg width="18" height="18" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
             <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 4h13M3 8h9m-9 4h6m4 0l4-4m0 0l4 4m-4-4v12" />
          </svg>
          Sortuj Strukturalnie (A-Z)
        </button>
      </div>

      {errorMsg && <p style={{ color: 'red' }}>Błąd bazy: {errorMsg}</p>}

      <BomFilters currentView={currentView} setCurrentView={setCurrentView} />

      <BomTable data={processedData} currentView={currentView} />
    </div>
  );
}