import SwiftUI

struct ControlPanelContentView: View {
    @EnvironmentObject var model: AppModel

    var body: some View {
        NavigationStack(path: $model.navigationPath) {
            TableOfContentsView(modules: Module.tableOfContents)
                .navigationTitle("VDOT XR Tool")
                .navigationDestination(for: Module.self) { module in
                    ModuleDetailView(module: module)
                        .navigationTitle(module.eyebrow)
                }
        }
    }
}

struct TableOfContentsView: View {
    @EnvironmentObject var model: AppModel
    let modules: [Module]

    var body: some View {
        List(modules) { module in
            Button {
                model.open(module)
            } label: {
                VStack(alignment: .leading, spacing: 4) {
                    Text(module.eyebrow)
                        .font(.caption)
                        .foregroundStyle(.secondary)

                    Text(module.title)
                        .font(.body)
                }
                .padding(.vertical, 6)
            }
        }
    }
}

struct ModuleDetailView: View {
    @EnvironmentObject var model: AppModel
    let module: Module

    var body: some View {
        VStack(spacing: 16) {
            Text(module.title)
                .font(.title2)

            Button("Trigger Unity Action") {
                CallCSharpCallback("action:\(module.id)")
            }

            Button("Back to Home") {
                model.goHome()
            }
        }
        .padding()
    }
}
