#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "HOUnitSpec.h"
#include "FXSignalGroupType.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType REQUIREMENTS
 *
 * Root of all requirements
 */
class ASAM_FIBEX_EXPORT FXRequirements
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXRequirements(FXFibex *fibex = Q_NULLPTR);
    ~FXRequirements();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element PORT-REQS */
    /** @todo element FUNCTION-REQS */
    /** @todo element SIGNAL-GROUPS */
    QHash<QString, FXSignalGroupType*> m_signalGroups;
    /** @todo element PDU-REQS */
};

} // FIBEX
} // ASAM
