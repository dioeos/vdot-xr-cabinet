import Foundation
import SwiftUI

@MainActor
final class AppModel: ObservableObject {
    @Published var navigationPath: [Module] = []

    func open(_ module: Module) {
        navigationPath.append(module)
        sendToUnity(module.unityCommand)
    }

    func goHome() {
        navigationPath.removeAll()
        sendToUnity("nav:home")
    }

    func handleUnityMessage(_ message: String) {
        // Example messages:
        // "nav:home"
        // "nav:module:diagnostics"

        if message == "nav:home" {
            navigationPath.removeAll()
            return
        }

        if message.hasPrefix("nav:module:") {
            let id = String(message.dropFirst("nav:module:".count))

            if let module = Module.tableOfContents.first(where: { $0.id == id }) {
                navigationPath = [module]
            }
        }
    }

    private func sendToUnity(_ message: String) {
        CallCSharpCallback(message)
    }
}
