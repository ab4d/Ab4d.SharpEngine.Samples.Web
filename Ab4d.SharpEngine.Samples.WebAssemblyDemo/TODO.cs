

// REMOVE MinCanvasInterop



// Implement CanvasInterop:
// - report size change
// - continues requestAnimationFrame
// - add mouse events

// How to load brotli compressed files from javascipt and when initializing dotnet
// How to add br files to the signed files (to blazor.boot.json)

// If we want to show loading progress, then we would need to use blazor.webassembly.js to load wasm files.
// There is:
//     function gt(e, t) {
//         const n = (e / t) * 100;
//         document.documentElement.style.setProperty("--blazor-load-percentage", `${n}%`),
//         document.documentElement.style.setProperty("--blazor-load-percentage-text", `"${Math.floor(n)}%"`);
//     }
// That sets the CSS variables --blazor-load-percentage and --blazor-load-percentage-text:
// .loading-progress {
//     position: relative;
//     display: block;
//     width: 8rem;
//     height: 8rem;
//     margin: 20vh auto 1rem auto;
// }
// 
//     .loading-progress circle {
//         fill: none;
//         stroke: #e0e0e0;
//         stroke-width: 0.6rem;
//         transform-origin: 50% 50%;
//         transform: rotate(-90deg);
//     }
// 
//         .loading-progress circle:last-child {
//             stroke: #1b6ec2;
//             stroke-dasharray: calc(3.141 * var(--blazor-load-percentage, 0%) * 0.8), 500%;
//             transition: stroke-dasharray 0.05s ease-in-out;
//         }
// 
// .loading-progress-text {
//     position: absolute;
//     text-align: center;
//     font-weight: bold;
//     inset: calc(20vh + 3.25rem) 0 auto 0.2rem;
// }
// 
//     .loading-progress-text:after {
//         content: var(--blazor-load-percentage-text, "Loading");
//     }
