// Biểu đồ trên trang in MB06/MB07 (file 09 mục 3): ECharts SVG renderer, tắt animation – dữ liệu từ khối JSON do server dựng.
import { veIn, so } from './charts/chart-factory.js';
import { BANG_MAU_DO } from './charts/chart-theme.js';

const nguon = document.getElementById('duLieuBieuDoIn');
const d = nguon ? JSON.parse(nguon.textContent) : null;
const $ = (id) => document.getElementById(id);
const chu = { fontSize: 10 };

if (d?.loai === 'MB06' && $('inBieuDoTheoNgay')) {
  veIn($('inBieuDoTheoNgay'), {
    grid: { left: 4, right: 8, top: 26, bottom: 4, containLabel: true },
    legend: { top: 0, left: 0, textStyle: chu },
    xAxis: { type: 'category', data: d.ngay, axisLabel: { ...chu, hideOverlap: true } },
    yAxis: { type: 'value', minInterval: 1, axisLabel: chu },
    series: [
      { name: 'Lượt mượn', type: 'bar', barMaxWidth: 10, data: d.muon },
      { name: 'Lượt gia hạn', type: 'bar', barMaxWidth: 10, data: d.giaHan, itemStyle: { color: BANG_MAU_DO[4] } },
      { name: 'Lượt trả', type: 'bar', barMaxWidth: 10, data: d.tra, itemStyle: { color: BANG_MAU_DO[3] } }
    ]
  });
}

if (d?.loai === 'MB07' && $('inBieuDoKhoanThu')) {
  veIn($('inBieuDoKhoanThu'), {
    grid: { left: 4, right: 8, top: 26, bottom: 4, containLabel: true },
    legend: { top: 0, left: 0, textStyle: chu },
    xAxis: { type: 'category', data: d.khoanThu, axisLabel: chu },
    yAxis: { type: 'value', minInterval: 1, axisLabel: chu },
    series: [
      { name: 'Đã thanh toán', type: 'bar', stack: 'tt', barMaxWidth: 28, data: d.daThanhToan,
        label: { show: true, position: 'inside', fontSize: 9, color: '#FFFFFF', formatter: (p) => (p.value ? so(p.value) : '') } },
      { name: 'Chưa thanh toán', type: 'bar', stack: 'tt', barMaxWidth: 28, data: d.chuaThanhToan, itemStyle: { color: BANG_MAU_DO[5] },
        label: { show: true, position: 'inside', fontSize: 9, formatter: (p) => (p.value ? so(p.value) : '') } }
    ]
  });
  veIn($('inBieuDoTyLe'), {
    legend: { bottom: 0, left: 'center', textStyle: chu, itemWidth: 10, itemHeight: 7 },
    series: [{ name: 'Khoản thu', type: 'pie', radius: ['38%', '62%'], center: ['50%', '42%'],
      label: { fontSize: 9, formatter: (p) => `${p.percent}%` }, data: d.tyLe }]
  });
}
